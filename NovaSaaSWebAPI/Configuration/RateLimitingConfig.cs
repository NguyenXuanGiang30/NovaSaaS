using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace NovaSaaS.WebApi.Configuration
{
    /// <summary>
    /// Rate Limiting Configuration - Giới hạn requests theo TenantId và Plan.
    /// Sử dụng Microsoft.AspNetCore.RateLimiting (.NET 7+).
    /// </summary>
    public static class RateLimitingConfig
    {
        // Policy Names
        public const string BasicPlanPolicy = "BasicPlan";
        public const string ProPlanPolicy = "ProPlan";
        public const string EnterprisePlanPolicy = "EnterprisePlan";
        public const string AIEndpointPolicy = "AIEndpoint";

        /// <summary>
        /// Cấu hình Rate Limiting services.
        /// </summary>
        public static IServiceCollection AddTenantRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                
                // ========================================
                // BASIC PLAN: Fixed Window - 60 requests/phút
                // ========================================
                options.AddPolicy(BasicPlanPolicy, context =>
                {
                    var tenantId = GetTenantId(context);
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: tenantId,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 60,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5
                        });
                });

                // ========================================
                // PRO PLAN: Token Bucket - 200 requests/phút với burst
                // ========================================
                options.AddPolicy(ProPlanPolicy, context =>
                {
                    var tenantId = GetTenantId(context);
                    return RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: tenantId,
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 200,
                            TokensPerPeriod = 50,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(15),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 10
                        });
                });

                // ========================================
                // ENTERPRISE PLAN: Token Bucket - 500 requests/phút
                // ========================================
                options.AddPolicy(EnterprisePlanPolicy, context =>
                {
                    var tenantId = GetTenantId(context);
                    return RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: tenantId,
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 500,
                            TokensPerPeriod = 125,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(15),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 20
                        });
                });

                // ========================================
                // AI ENDPOINT: Giới hạn nghiêm ngặt hơn cho AI services
                // 20 AI requests/phút cho Basic, 100 cho Pro/Enterprise
                // ========================================
                options.AddPolicy(AIEndpointPolicy, context =>
                {
                    var tenantId = GetTenantId(context);
                    var planId = GetPlanId(context);
                    
                    var limit = planId switch
                    {
                        "enterprise" => 100,
                        "pro" => 50,
                        _ => 20 // basic
                    };

                    return RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: $"ai_{tenantId}",
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = limit,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6, // 10-second segments
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 2
                        });
                });

                // Custom response khi bị rate limit
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";
                    
                    var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                        ? retryAfterValue.TotalSeconds
                        : 60;

                    var response = new
                    {
                        error = new
                        {
                            code = "ERR_RATE_LIMIT",
                            message = "Quá nhiều yêu cầu. Vui lòng thử lại sau.",
                            retryAfterSeconds = (int)retryAfter
                        }
                    };

                    await context.HttpContext.Response.WriteAsJsonAsync(response, token);
                };
            });

            return services;
        }

        private static string GetTenantId(HttpContext context)
        {
            // Lấy TenantId từ claims hoặc header
            var tenantClaim = context.User?.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim))
                return tenantClaim;

            // Fallback to header
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
                return headerValue.ToString();

            // Fallback to IP for unauthenticated requests
            return context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        }

        private static string GetPlanId(HttpContext context)
        {
            // Lấy PlanId từ claims
            return context.User?.FindFirst("plan_id")?.Value ?? "basic";
        }
    }
}
