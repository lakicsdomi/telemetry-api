using Prometheus;  // Prometheus for metrics collection, this will be the data source in Grafana
using TelemetryApi;

var builder = WebApplication.CreateBuilder(args);

// Register the telemetry service for DI
builder.Services.AddSingleton<ITelemetryService, TelemetryService>();

var app = builder.Build();

// Set up Prometheus metrics endpoint and middleware
app.UseHttpMetrics();
app.MapMetrics();

app.UseHttpsRedirection();

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