# Observability, Health, Tracing and Problem Details

## Purpose
Provide centralized operational visibility without coupling feature slices to a tracing vendor.

## Registration
Use `AddApplicationObservability(configuration)` to register:

- SQL Server readiness health check
- ASP.NET Core tracing
- `HttpClient` tracing
- SQL Client tracing
- ASP.NET Core and `HttpClient` metrics
- OTLP exporters when `OTEL_EXPORTER_OTLP_ENDPOINT` is configured

```csharp
services.AddHealthChecks()
  .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

var exportTelemetry = !string.IsNullOrWhiteSpace(
  configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

services.AddOpenTelemetry()
  .ConfigureResource(resource => resource.AddService("MinimalEP"))
  .WithTracing(tracing =>
  {
    tracing
      .AddAspNetCoreInstrumentation()
      .AddHttpClientInstrumentation()
      .AddSqlClientInstrumentation();

    if (exportTelemetry)
      tracing.AddOtlpExporter();
  });
```

A backend such as Jaeger should receive OTLP. Keep vendor-specific configuration at the hosting/infrastructure boundary.

## Trace correlation

- Preserve the incoming W3C `traceparent` header; ASP.NET Core creates a unique root trace when none is supplied.
- Use `Activity.Current?.TraceId` as the correlation identifier.
- Child spans for application work, `HttpClient`, and SQL must remain in the same trace.
- Include the trace ID in structured logs and centralized Problem Details responses.
- Never generate unrelated IDs in each layer when an active `Activity.TraceId` exists.
- Never attach access tokens, refresh tokens, passwords, full request bodies, or unnecessary personal data to spans or logs.

## Health endpoints

```csharp
app.MapHealthChecks(ApiRoutes.LiveHealth, new HealthCheckOptions
{
  Predicate = _ => false
}).AllowAnonymous();

app.MapHealthChecks(ApiRoutes.ReadyHealth, new HealthCheckOptions
{
  Predicate = check => check.Tags.Contains("ready")
}).AllowAnonymous();
```

- `/health/live` verifies only that the process is running.
- `/health/ready` verifies dependencies required to serve traffic, currently SQL Server.
- Both endpoints remain anonymous for orchestrator access and must not disclose secrets.

## Problem Details

Register `AddProblemDetails()` and call `UseExceptionHandler()`. Expected domain outcomes remain `Result<T>` values; centralized exception handling is for unexpected failures.

When customizing Problem Details, add the current trace ID to extensions so an API error can be correlated with logs and traces without exposing internal exception details.

## Verification

- Test that liveness and readiness endpoints are anonymous.
- Test expected readiness behavior against SQL Server.
- Verify exported traces contain one consistent trace ID across inbound HTTP, child spans, outbound HTTP, and SQL.
- Verify logs and Problem Details expose the same trace ID and contain no secrets.
