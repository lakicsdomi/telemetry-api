using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;

namespace TelemetryApi.Tests;

/// <summary>
/// Unit tests for the ExcludeMetricsLoggingInterceptor to ensure 100% code coverage.
/// Verifies that specific requests (favicon) and successful metric requests are silenced,
/// while keeping standard requests and failed metric requests logged.
/// </summary>
public class ExcludeMetricsLoggingInterceptorTests
{
    /// <summary>
    /// Verifies that any request to /favicon.ico disables logging immediately upon request.
    /// </summary>
    [Fact]
    public async Task OnRequestAsync_FaviconPath_DisablesLogging()
    {
        // Arrange
        var interceptor = new ExcludeMetricsLoggingInterceptor();
        var context = new DefaultHttpContext();
        context.Request.Path = "/favicon.ico";

        var logContext = new HttpLoggingInterceptorContext
        {
            HttpContext = context,
            // Set initial state to All to ensure the interceptor changes it to None
            LoggingFields = HttpLoggingFields.All
        };

        // Act
        await interceptor.OnRequestAsync(logContext);

        // Assert
        Assert.Equal(HttpLoggingFields.None, logContext.LoggingFields);
    }

    /// <summary>
    /// Verifies that requests to normal endpoints (like /status) do not disable logging upon request.
    /// </summary>
    [Fact]
    public async Task OnRequestAsync_NormalPath_KeepsLoggingEnabled()
    {
        // Arrange
        var interceptor = new ExcludeMetricsLoggingInterceptor();
        var context = new DefaultHttpContext();
        context.Request.Path = "/status";

        var logContext = new HttpLoggingInterceptorContext
        {
            HttpContext = context,
            LoggingFields = HttpLoggingFields.All
        };

        // Act
        await interceptor.OnRequestAsync(logContext);

        // Assert
        Assert.Equal(HttpLoggingFields.All, logContext.LoggingFields);
    }

    /// <summary>
    /// Verifies the response logic using various paths and status codes.
    /// Only a 200 OK response on the /metrics path should disable logging.
    /// </summary>
    [Theory]
    [InlineData("/metrics", 200, HttpLoggingFields.None)] // Successful scrape: mute
    [InlineData("/metrics", 404, HttpLoggingFields.All)]  // Failed scrape: log it
    [InlineData("/metrics", 500, HttpLoggingFields.All)]  // Server error on scrape: log it
    [InlineData("/status", 200, HttpLoggingFields.All)]   // Normal endpoint success: log it
    [InlineData("/api/data", 400, HttpLoggingFields.All)] // Normal endpoint error: log it
    public async Task OnResponseAsync_EvaluatesMetricsAndStatusCorrectly(string path, int statusCode, HttpLoggingFields expectedFields)
    {
        // Arrange
        var interceptor = new ExcludeMetricsLoggingInterceptor();
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.StatusCode = statusCode;

        var logContext = new HttpLoggingInterceptorContext
        {
            HttpContext = context,
            LoggingFields = HttpLoggingFields.All
        };

        // Act
        await interceptor.OnResponseAsync(logContext);

        // Assert
        Assert.Equal(expectedFields, logContext.LoggingFields);
    }
}