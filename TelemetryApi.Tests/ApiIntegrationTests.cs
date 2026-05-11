using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;

namespace TelemetryApi.Tests;

/// <summary>
/// Integration tests for the Telemetry API endpoints.
/// These tests verify the full HTTP pipeline, including middleware, 
/// routing, and authorization logic defined in Program.cs.
/// </summary>
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes the integration test factory.
    /// </summary>
    /// <param name="factory">The WebApplicationFactory provided by xUnit.</param>
    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Verifies that the root URL ("/") correctly redirects to the "/status" endpoint.
    /// Covers the redirect branch and location header validation.
    /// </summary>
    [Fact]
    public async Task Root_RedirectsToStatus()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/status", response.Headers.Location?.OriginalString);
    }

    /// <summary>
    /// Verifies that the /status endpoint returns a successful response and the expected JSON structure.
    /// </summary>
    [Fact]
    public async Task StatusEndpoint_ReturnsSuccessAndCorrectJson()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/status");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

        Assert.True(content.TryGetProperty("service", out var serviceProp));
        Assert.Equal("TelemetryApi", serviceProp.GetString());
    }

    /// <summary>
    /// Verifies that if no IP header is present and the connection IP is null 
    /// (which happens inherently in TestServer), the API correctly rejects the request.
    /// </summary>
    [Fact]
    public async Task StatsEndpoint_ReturnsNotFound_WhenNoIpAvailable()
    {
        // Arrange
        var allowedIp = "127.0.0.1";
        var client = CreateConfiguredClient(allowedIp);

        // Act - No X-Forwarded-For header added, and TestServer has no RemoteIpAddress
        var response = await client.GetAsync("/stats/ips");

        // Assert - Mivel semmilyen IP-t nem talált a rendszer, a biztonsági 404 a helyes válasz
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Verifies that /stats/ips returns 404 when the IP is not in the allowed list.
    /// Also covers the case where clientIp might be null or empty.
    /// </summary>
    [Fact]
    public async Task StatsEndpoint_ReturnsNotFound_WhenIpIsNotAllowed()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "9.9.9.9");

        // Act
        var response = await client.GetAsync("/stats/ips");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Verifies that the configuration parser handles multiple IPs, 
    /// extra whitespaces, and empty entries correctly.
    /// </summary>
    [Fact]
    public async Task StatsEndpoint_HandlesComplexAllowedIpString()
    {
        // Arrange
        var configIps = " 1.1.1.1 , ,  127.0.0.1 ";
        var client = CreateConfiguredClient(configIps);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");

        // Act
        var response = await client.GetAsync("/stats/ips");

        // Assert
        response.EnsureSuccessStatusCode();
        var stats = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        Assert.NotNull(stats);
        Assert.True(stats.ContainsKey("127.0.0.1"));
    }

    /// <summary>
    /// Verifies that request counts are correctly incremented across multiple calls.
    /// </summary>
    [Fact]
    public async Task StatsEndpoint_IncrementsRequestCounts()
    {
        // Arrange
        var ip = "10.0.0.1";
        var client = CreateConfiguredClient(ip);
        client.DefaultRequestHeaders.Add("X-Forwarded-For", ip);

        // Act
        await client.GetAsync("/status");
        await client.GetAsync("/status");
        var response = await client.GetAsync("/stats/ips");
        var stats = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(3, stats[ip]); // 2 status calls + 1 stats call
    }

    /// <summary>
    /// Helper method to create a test client with custom AllowedIps configuration.
    /// </summary>
    private HttpClient CreateConfiguredClient(string allowedIps)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("AllowedIps", allowedIps)
                });
            });
        }).CreateClient();
    }
}