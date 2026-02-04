using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace NovaSaaS.WebApi.Configuration
{
    /// <summary>
    /// OpenTelemetry Configuration - Distributed tracing và metrics.
    /// Cho phép theo dõi request từ đầu đến cuối qua các services.
    /// </summary>
    public static class OpenTelemetryConfig
    {
        private const string ServiceName = "NovaSaaS.WebAPI";
        private const string ServiceVersion = "1.0.0";

        /// <summary>
        /// Cấu hình OpenTelemetry cho ứng dụng.
        /// </summary>
        public static IServiceCollection AddNovaSaaSOpenTelemetry(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(
                        serviceName: ServiceName,
                        serviceVersion: ServiceVersion)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["environment"] = configuration["Environment"] ?? "production",
                        ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production"
                    }))
                .WithTracing(tracing =>
                {
                    tracing
                        // ASP.NET Core instrumentation
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.EnrichWithHttpRequest = (activity, request) =>
                            {
                                // Đính kèm TenantId vào trace
                                var tenantId = request.HttpContext.User?.FindFirst("tenant_id")?.Value;
                                if (!string.IsNullOrEmpty(tenantId))
                                {
                                    activity.SetTag("tenant.id", tenantId);
                                }

                                // Request info
                                activity.SetTag("http.request.path", request.Path);
                                activity.SetTag("http.request.method", request.Method);
                            };
                            options.EnrichWithHttpResponse = (activity, response) =>
                            {
                                activity.SetTag("http.response.status_code", response.StatusCode);
                            };
                        })
                        // HTTP Client instrumentation
                        .AddHttpClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.EnrichWithHttpRequestMessage = (activity, request) =>
                            {
                                activity.SetTag("http.url", request.RequestUri?.ToString());
                            };
                        })
                        // Custom source for AI services
                        .AddSource("NovaSaaS.AI")
                        .AddSource("NovaSaaS.RAG")
                        .AddSource("NovaSaaS.DB");

                    // Export to OTLP if configured
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                    {
                        tracing.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                        });
                    }
                    else
                    {
                        // Console exporter for development
                        tracing.AddConsoleExporter();
                    }
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        // Custom meters
                        .AddMeter("NovaSaaS.AI")
                        .AddMeter("NovaSaaS.Business");

                    if (!string.IsNullOrEmpty(otlpEndpoint))
                    {
                        metrics.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otlpEndpoint);
                        });
                    }
                    else
                    {
                        metrics.AddConsoleExporter();
                    }
                });

            return services;
        }
    }
}
