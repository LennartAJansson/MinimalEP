namespace MinimalEP.Infrastructure.Observability;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public static class ObservabilityExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddApplicationObservability(IConfiguration configuration)
    {
      services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

      var exportTelemetry = !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

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
        })
        .WithMetrics(metrics =>
        {
          metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

          if (exportTelemetry)
            metrics.AddOtlpExporter();
        });

      return services;
    }
  }
}
