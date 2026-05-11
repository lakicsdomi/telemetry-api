using Prometheus;  // Prometheus for metrics collection, this will be the data source in Grafana
using TelemetryApi;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

// Add simple console logging with timestamps and single-line format
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

// Register the telemetry service for DI
builder.Services.AddSingleton<ITelemetryService, TelemetryService>();

// Standard HTTP Logging for Docker (Logs to stdout, includes IP, safely redacts headers)
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestMethod |
                            HttpLoggingFields.RequestPropertiesAndHeaders |
                            HttpLoggingFields.ResponseStatusCode |
                            HttpLoggingFields.RequestPath;

    // Log the real client IP address (from X-Forwarded-For or RemoteIpAddress)
    logging.RequestHeaders.Add("X-Forwarded-For");
    // Avoid logging Prometheus scrape requests to prevent log spam
    logging.CombineLogs = true;

    
});

var app = builder.Build();

// Thread safe dictionary to track total requests per IP address
var ipRequestCounts = new System.Collections.Concurrent.ConcurrentDictionary<string, int>();

// Middleware to count requests per IP address
app.Use(async (context, next) =>
{
    var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    if (string.IsNullOrEmpty(ipAddress))
    {
        ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
    ipRequestCounts.AddOrUpdate(ipAddress, 1, (key, count) => count + 1);
    await next.Invoke();
}); 

// Secure stats endpoint
app.MapGet("/stats/ips", (IConfiguration config, HttpContext context) =>
{
    // Get the allowed IPs from environment variable
    var allowedIpsRaw = config["AllowedIps"] ?? "";
    var allowedIps = allowedIpsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(ip => ip.Trim());

    var clientIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                   ?? context.Connection.RemoteIpAddress?.ToString();

    if (string.IsNullOrEmpty(clientIp) || !allowedIps.Contains(clientIp))
    {
        return Results.NotFound(); // Return 404 to hide the existence of this endpoint from unauthorized users
    }

    return Results.Ok(ipRequestCounts.OrderByDescending(x => x.Value));
});

// app.UseHttpsRedirection();

// HTTP logging middleware
app.UseHttpLogging();

app.UseRouting();

// Set up Prometheus metrics endpoint and middleware
app.UseHttpMetrics();

// The metrics endpoint can only be accessed from the 9091 internal port.
app.MapMetrics().RequireHost("*:9091");

app.MapGet("/status", (ITelemetryService telemetryService) =>
{
    // Return an object instead of a raw string for better JSON structure and testability
    return Results.Ok(new { service = "TelemetryApi", status = telemetryService.GetStatus() });
});

// Redirect root to /status for convenience
app.MapGet("/", () => Results.Redirect("/status"));

app.Run();

/// <summary>
/// The main entry point for the Telemetry API application.
/// Has minimal setup to create a web application that serves telemetry data at the /status endpoint and exposes Prometheus metrics at /metrics.
/// </summary>
public partial class Program { }