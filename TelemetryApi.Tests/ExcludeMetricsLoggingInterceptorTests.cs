using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;

namespace TelemetryApi.Tests;

/// <summary>
/// Unit tests for the ExcludeMetricsLoggingInterceptor to ensure 100% code coverage.
/// Verifies that specific requests (favicon and metrics) are silenced,
/// while keeping standard requests logged.
/// </summary>
public class ExcludeMetricsLoggingInterceptorTests
{
    /// <summary>
    /// Verifies that any request to /favicon.ico or /metrics disables logging immediately upon request.
    /// </summary>
    [Theory]
    [InlineData("/favicon.ico", HttpLoggingFields.None)]
    [InlineData("/metrics", HttpLoggingFields.None)]
    [InlineData("/status", HttpLoggingFields.All)]
    public async Task OnRequestAsync_EvaluatesPathsCorrectly(string path, HttpLoggingFields expectedFields)
    {
        // Arrange
        var interceptor = new ExcludeMetricsLoggingInterceptor();
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var logContext = new HttpLoggingInterceptorContext
        {
            HttpContext = context,
            // Set initial state to All to ensure the interceptor changes it to None if needed
            LoggingFields = HttpLoggingFields.All
        };

        // Act
        await interceptor.OnRequestAsync(logContext);

        // Assert
        Assert.Equal(expectedFields, logContext.LoggingFields);
    }

    /// <summary>
    /// Verifies that the OnResponseAsync method executes without errors and does not alter the context.
    /// </summary>
    [Fact]
    public async Task OnResponseAsync_DoesNothing_ExecutesSuccessfully()
    {
        // Arrange
        var interceptor = new ExcludeMetricsLoggingInterceptor();
        var logContext = new HttpLoggingInterceptorContext
        {
            // Set an initial state to verify it remains unchanged
            LoggingFields = HttpLoggingFields.All 
        };

        // Act
        // Since the method returns a ValueTask (effectively void), we just await it.
        // If it doesn't throw an exception, the execution is successful.
        await interceptor.OnResponseAsync(logContext);

        // Assert
        // We verify that the original logging fields were not modified by the response method
        Assert.Equal(HttpLoggingFields.All, logContext.LoggingFields);
    }
}