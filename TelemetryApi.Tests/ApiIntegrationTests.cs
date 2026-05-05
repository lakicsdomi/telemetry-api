// ApiIntegrationTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace TelemetryApi.Tests;

/// <summary>
/// Integration tests for the Telemetry API endpoints. 
/// These tests will start up the actual application 
/// and make HTTP requests to verify that the endpoints
/// are working as expected, including the /status endpoint
/// which is critical for our telemetry data.
/// </summary>
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    /// <summary>
    /// Constructor that initializes the HttpClient using the WebApplicationFactory.
    /// </summary>
    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Test to verify that the /status endpoint returns a successful response and contains the expected JSON structure.
    /// </summary>
    [Fact]
    public async Task StatusEndpoint_ReturnsSuccessAndCorrectJson()
    {
        // Act
        var response = await _client.GetAsync("/status");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<dynamic>();

        Assert.NotNull(content);
        // Check if our real data fields exist
        string? serviceName = content?.GetProperty("service").GetString();
        Assert.Equal("TelemetryApi", serviceName);
    }
}