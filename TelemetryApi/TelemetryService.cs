namespace TelemetryApi;

/// <summary>
/// Interface for telemetry data provider
/// </summary>
public interface ITelemetryService
{
    string GetStatus();
}

/// <summary>
/// Implementation of the telemetry service
/// </summary>
public class TelemetryService : ITelemetryService
{
    /// <summary>
    /// Gets the current status of the service
    /// </summary>
    /// <returns>
    /// String representing the current status of the service, e.g. "Online", "Offline", "Degraded"
    /// </returns>
    public string GetStatus()
    {
        // Determine the status of the service based on internal logic, health checks, or other criteria
        // For demonstration purposes, we'll return "Online" as the status
        return "Online";
    }
}