using Prometheus;  // Prometheus for metrics collection, this will be the data source in Grafana
using TelemetryApi;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

// Register the telemetry service for DI
builder.Services.AddSingleton<ITelemetryService, TelemetryService>();

// Standard HTTP Logging for Docker (Logs to stdout, includes IP, safely redacts headers)
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders |
                            HttpLoggingFields.ResponseStatusCode;
    // Avoid logging Prometheus scrape requests to prevent log spam
    logging.CombineLogs = true;
});

var app = builder.Build();

app.UseHttpsRedirection();

// HTTP logging middleware
app.UseHttpLogging();

app.UseRouting();

// Set up Prometheus metrics endpoint and middleware
app.UseHttpMetrics();

// The metrics endpoint can only be accessed from the 9091 internal port.
app.MapMetrics().RequireHost("*:9091");

app.MapGet("/status", (ITelemetryService telemetryService) =>
{
    return Results.Ok(telemetryService.GetStatus());
});

// Redirect root to /status for convenience
app.MapGet("/", () => Results.Redirect("/status"));

app.Run();

/// <summary>
/// The main entry point for the Telemetry API application.
/// Has minimal setup to create a web application that serves telemetry data at the /status endpoint and exposes Prometheus metrics at /metrics.
/// </summary>
public partial class Program { }