using Moq;

namespace TelemetryApi.Tests;

/// <summary>
/// Unit tests for the Status endpoint of the Telemetry API.
/// </summary>
public class StatusEndpointTests
{

    /// <summary>
    /// Test to verify that the GetStatus method of the ITelemetryService returns the expected mocked value.
    /// </summary>
    [Fact]
    public void GetStatus_ShouldReturnMockedValue()
    {
        // Arrange: Create a mock of the service
        var mockService = new Mock<ITelemetryService>();

        // Setup the mock to return a specific string when GetStatus is called
        mockService.Setup(x => x.GetStatus()).Returns("Mocked-Online");

        // Act: Call the method on the mocked object
        var result = mockService.Object.GetStatus();

        // Assert: Verify that the result matches our mocked setup
        Assert.Equal("Mocked-Online", result);
    }
}