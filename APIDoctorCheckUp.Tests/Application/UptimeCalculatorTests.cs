using APIDoctorCheckUp.Application.Interfaces;
using APIDoctorCheckUp.Application.Services;
using APIDoctorCheckUp.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace APIDoctorCheckUp.Tests.Application;

public class UptimeCalculatorTests
{
    private readonly Mock<ICheckResultRepository> _repoMock;
    private readonly UptimeCalculator _sut;

    public UptimeCalculatorTests()
    {
        _repoMock = new Mock<ICheckResultRepository>();
        _sut      = new UptimeCalculator(_repoMock.Object, NullLogger<UptimeCalculator>.Instance);
    }

    private static CheckResult MakeResult(bool isSuccess, int hoursAgo) => new()
    {
        IsSuccess  = isSuccess,
        CheckedAt  = DateTime.UtcNow.AddHours(-hoursAgo),
        EndpointId = 1
    };

    [Fact]
    public async Task Returns100_WhenAllChecksSucceed()
    {
        var results = Enumerable.Range(1, 10)
            .Select(i => MakeResult(isSuccess: true, hoursAgo: i))
            .ToList();

        _repoMock
            .Setup(r => r.GetByEndpointIdAsync(1, 10000, default))
            .ReturnsAsync(results);

        var uptime = await _sut.CalculateAsync(1, 24);

        Assert.Equal(100.0, uptime);
    }

    [Fact]
    public async Task Returns0_WhenAllChecksFail()
    {
        var results = Enumerable.Range(1, 10)
            .Select(i => MakeResult(isSuccess: false, hoursAgo: i))
            .ToList();

        _repoMock
            .Setup(r => r.GetByEndpointIdAsync(1, 10000, default))
            .ReturnsAsync(results);

        var uptime = await _sut.CalculateAsync(1, 24);

        Assert.Equal(0.0, uptime);
    }

    [Fact]
    public async Task Returns50_WhenHalfChecksFail()
    {
        var results = Enumerable.Range(1, 10)
            .Select(i => MakeResult(isSuccess: i % 2 == 0, hoursAgo: i))
            .ToList();

        _repoMock
            .Setup(r => r.GetByEndpointIdAsync(1, 10000, default))
            .ReturnsAsync(results);

        var uptime = await _sut.CalculateAsync(1, 24);

        Assert.Equal(50.0, uptime);
    }

    [Fact]
    public async Task Returns0_WhenNoResultsExist()
    {
        _repoMock
            .Setup(r => r.GetByEndpointIdAsync(1, 10000, default))
            .ReturnsAsync(new List<CheckResult>());

        var uptime = await _sut.CalculateAsync(1, 24);

        Assert.Equal(0.0, uptime);
    }

    [Fact]
    public async Task ExcludesResultsOutsideTimeWindow()
    {
        var results = new List<CheckResult>
        {
            MakeResult(isSuccess: true,  hoursAgo: 1),
            MakeResult(isSuccess: true,  hoursAgo: 2),
            MakeResult(isSuccess: false, hoursAgo: 48), // outside 24h window
            MakeResult(isSuccess: false, hoursAgo: 72)  // outside 24h window
        };

        _repoMock
            .Setup(r => r.GetByEndpointIdAsync(1, 10000, default))
            .ReturnsAsync(results);

        var uptime = await _sut.CalculateAsync(1, 24);

        // Only the 2 results within the window count, both successful
        Assert.Equal(100.0, uptime);
    }

    [Fact]
    public async Task ReturnsRoundedPercentage()
    {
        // 2 success out of 3 = 66.67%
        var results = new List<CheckResult>
        {
            MakeResult(isSuccess: true,  hoursAgo: 1),
            MakeResult(isSuccess: true,  hoursAgo: 2),
            MakeResult(isSuccess: false, hoursAgo: 3)
        };

        _repoMock
            .Setup(r => r.GetByEndpointIdAsync(1, 10000, default))
            .ReturnsAsync(results);

        var uptime = await _sut.CalculateAsync(1, 24);

        Assert.Equal(66.67, uptime);
    }
}
