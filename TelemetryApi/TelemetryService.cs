using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TelemetryApi;

/// <summary>
/// Interface for telemetry data provider
/// </summary>
public interface ITelemetryService
{
    object GetStatus();
}

/// <summary>
/// Implementation of the telemetry service
/// </summary>
public class TelemetryService : ITelemetryService
{
    private readonly string _version = "1.0.1";
    private readonly DateTime _startTime = DateTime.UtcNow;

    /// <summary>
    /// Gets the current status of the service, including version, uptime, OS info, and memory usage.
    /// </summary>
    public object GetStatus()
    {

        var process = Process.GetCurrentProcess();

        return new
        {
            Service = "TelemetryApi",
            Version = _version,
            Status = "Healthy",
            Uptime = DateTime.UtcNow - _startTime,
            OS = RuntimeInformation.OSDescription,
            // Memory info in MB
            MemoryUsageMB = process.WorkingSet64 / 1024 / 1024,
            Timestamp = DateTime.UtcNow
        };
    }
}