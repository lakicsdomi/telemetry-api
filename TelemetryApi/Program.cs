using Prometheus;  // Prometheus for metrics collection, this will be the data source in Grafana
using TelemetryApi;
using Microsoft.AspNetCore.HttpLogging;  // For logging HTTP requests and responses
using Microsoft.AspNetCore.HttpOverrides; // For handling X-Forwarded-For headers correctly when behind a reverse proxy

var builder = WebApplication.CreateBuilder(args);

// Add simple console logging with timestamps and single-line format
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = false;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});

// Register the telemetry service for DI
builder.Services.AddSingleton<ITelemetryService, TelemetryService>();


// Forwarded Headers configuration
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    // Check for X-Forwarded-For and X-Forwarded-Proto headers
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // ON the Docker network, the proxy (Nginx) IP can change, so we clear the security restrictions
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Standard HTTP Logging for Docker (Logs to stdout, includes IP, safely redacts headers)
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestMethod |
                            HttpLoggingFields.RequestPath |
                            HttpLoggingFields.ResponseStatusCode |
                            HttpLoggingFields.RequestProperties |
                            HttpLoggingFields.RequestHeaders;

    // Specifically explicitly log only this header to see the real IP
    logging.RequestHeaders.Add("X-Forwarded-For");
    logging.CombineLogs = true;
});

var app = builder.Build();

// Log the request while the X-Forwarded-For header still exists
app.UseHttpLogging();

// Middleware to get the correct client IP address when behind a reverse proxy.
app.UseForwardedHeaders();

// Thread safe dictionary to track total requests per IP address
var ipRequestCounts = new System.Collections.Concurrent.ConcurrentDictionary<string, int>();

// Middleware to count requests per IP address
app.Use(async (context, next) =>
{
    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    if (clientIp != "::1" && clientIp != "127.0.0.1")
    {
        ipRequestCounts.AddOrUpdate(clientIp, 1, (key, count) => count + 1);
    }
    await next.Invoke();
});

// Secure stats endpoint
app.MapGet("/stats/ips", (IConfiguration config, HttpContext context) =>
{
    var allowedIpsRaw = config["AllowedIps"] ?? "";
    var allowedIps = allowedIpsRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(ip => ip.Split('/')[0].Trim()); // CIDR /32

    var clientIp = context.Connection.RemoteIpAddress?.ToString();

    if (string.IsNullOrEmpty(clientIp) || !allowedIps.Contains(clientIp))
    {
        return Results.NotFound();
    }

    var result = ipRequestCounts.OrderByDescending(x => x.Value)
                                .ToDictionary(x => x.Key, x => x.Value);

    return Results.Ok(result);
});

// app.UseHttpsRedirection();

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