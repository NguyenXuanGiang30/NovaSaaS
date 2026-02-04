using Microsoft.Extensions.Diagnostics.HealthChecks;
using NovaSaaS.Application.Interfaces.AI;

namespace NovaSaaS.WebApi.HealthChecks
{
    /// <summary>
    /// GeminiHealthCheck - Kiểm tra kết nối Gemini AI API.
    /// </summary>
    public class GeminiHealthCheck : IHealthCheck
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly ILogger<GeminiHealthCheck> _logger;

        public GeminiHealthCheck(
            IEmbeddingService embeddingService,
            ILogger<GeminiHealthCheck> logger)
        {
            _embeddingService = embeddingService;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Test với một câu đơn giản
                var testEmbedding = await _embeddingService.GenerateEmbeddingAsync("health check test");
                stopwatch.Stop();

                if (testEmbedding != null && testEmbedding.Length > 0)
                {
                    var responseTime = stopwatch.ElapsedMilliseconds;
                    
                    // Warning nếu > 3 giây
                    if (responseTime > 3000)
                    {
                        return HealthCheckResult.Degraded(
                            $"Gemini API responding slowly: {responseTime}ms",
                            data: new Dictionary<string, object>
                            {
                                { "response_time_ms", responseTime },
                                { "embedding_dimensions", testEmbedding.Length }
                            });
                    }

                    return HealthCheckResult.Healthy(
                        $"Gemini API OK: {responseTime}ms",
                        data: new Dictionary<string, object>
                        {
                            { "response_time_ms", responseTime },
                            { "embedding_dimensions", testEmbedding.Length }
                        });
                }

                return HealthCheckResult.Unhealthy("Gemini API returned empty embedding");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(ex, "Gemini health check failed");

                return HealthCheckResult.Unhealthy(
                    $"Gemini API error: {ex.Message}",
                    exception: ex,
                    data: new Dictionary<string, object>
                    {
                        { "response_time_ms", stopwatch.ElapsedMilliseconds },
                        { "error_type", ex.GetType().Name }
                    });
            }
        }
    }
}
