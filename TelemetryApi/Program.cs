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

public partial class Program { }