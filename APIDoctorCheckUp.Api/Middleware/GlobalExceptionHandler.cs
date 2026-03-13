using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace APIDoctorCheckUp.Api.Middleware;

/// <summary>
/// Catches all unhandled exceptions that bubble up through the middleware
/// pipeline and returns a consistent RFC 9110 ProblemDetails JSON response.
/// Without this, unhandled exceptions produce HTML stack traces in Development
/// and empty 500 bodies in Production — neither is acceptable for a public API.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the full exception with stack trace so it appears in the
        // application logs even though we return a sanitised response to the client.
        _logger.LogError(
            exception,
            "Unhandled exception for {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title  = "An unexpected error occurred",
            // RFC 9110 reference for 500 Internal Server Error
            Type   = "https://tools.ietf.org/html/rfc9110#section-15.6.1"
        };

        // Include the trace ID so a developer can correlate the 500 response
        // with the full stack trace in the application logs.
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Returning true tells the framework we handled the exception.
        // The framework will not attempt any further exception handling.
        return true;
    }
}
