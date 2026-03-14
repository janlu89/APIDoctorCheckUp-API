using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Domain.Entities;
using APIDoctorCheckUp.Domain.Enums;
using APIDoctorCheckUp.Infrastructure.BackgroundServices;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace APIDoctorCheckUp.Tests.Infrastructure;

public class AlertEvaluatorTests
{
    private readonly Mock<ICheckResultRepository> _checkResultsMock;
    private readonly Mock<IIncidentRepository>    _incidentsMock;
    private readonly AlertEvaluator               _sut;

    public AlertEvaluatorTests()
    {
        _checkResultsMock = new Mock<ICheckResultRepository>();
        _incidentsMock    = new Mock<IIncidentRepository>();
        _sut              = new AlertEvaluator(
            _checkResultsMock.Object,
            _incidentsMock.Object,
            NullLogger<AlertEvaluator>.Instance);
    }

    private static MonitoredEndpoint MakeEndpoint(
        EndpointStatus currentStatus = EndpointStatus.Unknown,
        int warningMs  = 1000,
        int criticalMs = 3000,
        int failuresDown = 3) => new()
    {
        Id            = 1,
        Name          = "Test Endpoint",
        CurrentStatus = currentStatus,
        AlertThreshold = new AlertThreshold
        {
            ResponseTimeWarningMs   = warningMs,
            ResponseTimeCriticalMs  = criticalMs,
            ConsecutiveFailuresDown = failuresDown
        }
    };

    private static CheckResult MakeResult(
        bool isSuccess,
        long responseTimeMs = 100,
        int? statusCode = 200) => new()
    {
        EndpointId     = 1,
        IsSuccess      = isSuccess,
        ResponseTimeMs = responseTimeMs,
        StatusCode     = statusCode,
        CheckedAt      = DateTime.UtcNow
    };

    // -- No threshold fallback ------------------------------------------------

    [Fact]
    public async Task ReturnsUp_WhenNoThresholdAndCheckSucceeds()
    {
        var endpoint = new MonitoredEndpoint { Id = 1, Name = "Test", AlertThreshold = null };
        var result   = MakeResult(isSuccess: true);

        var status = await _sut.EvaluateAsync(endpoint, result);

        Assert.Equal(EndpointStatus.Up, status);
    }

    [Fact]
    public async Task ReturnsDown_WhenNoThresholdAndCheckFails()
    {
        var endpoint = new MonitoredEndpoint { Id = 1, Name = "Test", AlertThreshold = null };
        var result   = MakeResult(isSuccess: false);

        var status = await _sut.EvaluateAsync(endpoint, result);

        Assert.Equal(EndpointStatus.Down, status);
    }

    // -- Successful check status transitions ----------------------------------

    [Fact]
    public async Task ReturnsUp_WhenCheckSucceedsAndResponseTimeBelowWarning()
    {
        var endpoint = MakeEndpoint(warningMs: 1000);
        var result   = MakeResult(isSuccess: true, responseTimeMs: 500);

        var status = await _sut.EvaluateAsync(endpoint, result);

        Assert.Equal(EndpointStatus.Up, status);
    }

    [Fact]
    public async Task ReturnsDegraded_WhenResponseTimeExceedsWarningThreshold()
    {
        var endpoint = MakeEndpoint(warningMs: 1000, criticalMs: 3000);
        var result   = MakeResult(isSuccess: true, responseTimeMs: 1500);

        var status = await _sut.EvaluateAsync(endpoint, result);

        Assert.Equal(EndpointStatus.Degraded, status);
    }

    [Fact]
    public async Task ReturnsDown_WhenResponseTimeExceedsCriticalThreshold()
    {
        var endpoint = MakeEndpoint(warningMs: 1000, criticalMs: 3000);
        var result   = MakeResult(isSuccess: true, responseTimeMs: 4000);

        var status = await _sut.EvaluateAsync(endpoint, result);

        Assert.Equal(EndpointStatus.Down, status);
    }

    // -- Failed check consecutive failure counting ----------------------------

    [Fact]
    public async Task ReturnsDown_WhenConsecutiveFailuresReachThreshold()
    {
        var endpoint = MakeEndpoint(
            currentStatus: EndpointStatus.Up,
            failuresDown: 3);
        var result = MakeResult(isSuccess: false);

        _checkResultsMock
            .Setup(r => r.GetConsecutiveFailureCountAsync(1, default))
            .ReturnsAsync(3);

        _incidentsMock
            .Setup(r => r.GetOpenIncidentAsync(1, default))
            .ReturnsAsync((Incident?)null);

        _incidentsMock
            .Setup(r => r.AddAsync(It.IsAny<Incident>(), default))
            .ReturnsAsync(new Incident());

        var status = await _sut.EvaluateAsync(endpoint, result);

        Assert.Equal(EndpointStatus.Down, status);
    }

    [Fact]
    public async Task RetainsPreviousStatus_WhenFailuresBelowThreshold()
    {
        var endpoint = MakeEndpoint(
            currentStatus: EndpointStatus.Up,
            failuresDown: 3);
        var result = MakeResult(isSuccess: false);

        _checkResultsMock
            .Setup(r => r.GetConsecutiveFailureCountAsync(1, default))
            .ReturnsAsync(1); // only 1 failure, threshold is 3

        var status = await _sut.EvaluateAsync(endpoint, result);

        // Should retain Up status since we haven't hit the threshold
        Assert.Equal(EndpointStatus.Up, status);
    }

    // -- Incident lifecycle ---------------------------------------------------

    [Fact]
    public async Task OpensIncident_WhenEndpointTransitionsToDown()
    {
        var endpoint = MakeEndpoint(
            currentStatus: EndpointStatus.Up,
            failuresDown: 3);
        var result = MakeResult(isSuccess: false);

        _checkResultsMock
            .Setup(r => r.GetConsecutiveFailureCountAsync(1, default))
            .ReturnsAsync(3);

        _incidentsMock
            .Setup(r => r.GetOpenIncidentAsync(1, default))
            .ReturnsAsync((Incident?)null);

        _incidentsMock
            .Setup(r => r.AddAsync(It.IsAny<Incident>(), default))
            .ReturnsAsync(new Incident());

        await _sut.EvaluateAsync(endpoint, result);

        _incidentsMock.Verify(
            r => r.AddAsync(It.IsAny<Incident>(), default),
            Times.Once);
    }

    [Fact]
    public async Task ClosesIncident_WhenEndpointRecoverFromDown()
    {
        var endpoint = MakeEndpoint(currentStatus: EndpointStatus.Down);
        var result   = MakeResult(isSuccess: true, responseTimeMs: 100);

        var openIncident = new Incident
        {
            Id         = 1,
            EndpointId = 1,
            StartedAt  = DateTime.UtcNow.AddMinutes(-10)
        };

        _incidentsMock
            .Setup(r => r.GetOpenIncidentAsync(1, default))
            .ReturnsAsync(openIncident);

        _incidentsMock
            .Setup(r => r.UpdateAsync(It.IsAny<Incident>(), default))
            .Returns(Task.CompletedTask);

        await _sut.EvaluateAsync(endpoint, result);

        _incidentsMock.Verify(
            r => r.UpdateAsync(
                It.Is<Incident>(i => i.ResolvedAt != null),
                default),
            Times.Once);
    }

    [Fact]
    public async Task DoesNotOpenDuplicateIncident_WhenAlreadyOpen()
    {
        var endpoint = MakeEndpoint(
            currentStatus: EndpointStatus.Up,
            failuresDown: 3);
        var result = MakeResult(isSuccess: false);

        _checkResultsMock
            .Setup(r => r.GetConsecutiveFailureCountAsync(1, default))
            .ReturnsAsync(3);

        // An incident already exists
        _incidentsMock
            .Setup(r => r.GetOpenIncidentAsync(1, default))
            .ReturnsAsync(new Incident { Id = 1, EndpointId = 1 });

        await _sut.EvaluateAsync(endpoint, result);

        // AddAsync should never be called since incident already exists
        _incidentsMock.Verify(
            r => r.AddAsync(It.IsAny<Incident>(), default),
            Times.Never);
    }
}
