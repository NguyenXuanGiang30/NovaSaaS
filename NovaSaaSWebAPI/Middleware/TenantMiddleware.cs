using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NovaSaaS.Domain.Entities.Master;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.Infrastructure.Persistence;
using System.Security.Claims;

namespace NovaSaaSWebAPI.Middleware
{
    /// <summary>
    /// TenantMiddleware - "Bộ điều hướng trung tâm" (Traffic Controller)
    /// 
    /// Quy trình 4 bước:
    /// 1. Đánh chặn (Intercept) - Tạm dừng request để kiểm tra danh tính
    /// 2. Trích xuất (Extract) - Tìm Tenant ID từ Header/Subdomain/JWT
    /// 3. Xác thực (Validate) - Kiểm tra Tenant có tồn tại và đang hoạt động
    /// 4. Thiết lập ngữ cảnh (Context Injection) - Inject vào ITenantService
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        // Các đường dẫn được bypass (không cần xác thực Tenant)
        private static readonly string[] BypassPaths = new[]
        {
            "/api/registration",    // Đăng ký tenant mới
            "/api/master",          // Master admin endpoints
            "/health",              // Health check
            "/openapi",             // OpenAPI/Swagger
            "/swagger"              // Swagger UI
        };

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context, 
            ITenantService tenantService, 
            ApplicationDbContext dbContext,
            IMemoryCache cache)
        {
            // ========================================
            // BƯỚC 0: Kiểm tra Bypass Paths
            // ========================================
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            
            if (ShouldBypass(path))
            {
                _logger.LogDebug("🔓 Bypass tenant check for path: {Path}", path);
                await _next(context);
                return;
            }

            // ========================================
            // BƯỚC 1: ĐÁNH CHẶN (Intercept)
            // ========================================
            _logger.LogDebug("🚦 Intercepting request: {Method} {Path}", 
                context.Request.Method, context.Request.Path);

            // ========================================
            // BƯỚC 2: TRÍCH XUẤT (Extract)
            // ========================================
            var tenantIdentifier = ExtractTenantIdentifier(context);

            if (string.IsNullOrEmpty(tenantIdentifier))
            {
                // Request không có tenant identifier
                // Có thể là public endpoint hoặc master endpoint
                _logger.LogDebug("⚪ No tenant identifier found, proceeding with public schema");
                await _next(context);
                return;
            }

            _logger.LogDebug("🔍 Extracted tenant identifier: {TenantId}", tenantIdentifier);

            // ========================================
            // BƯỚC 3: XÁC THỰC (Validate)
            // ========================================
            var tenant = await ValidateTenantAsync(tenantIdentifier, dbContext, cache);

            if (tenant == null)
            {
                _logger.LogWarning("🚫 Tenant not found: {TenantId}", tenantIdentifier);
                
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Tenant not found",
                    message = $"Tenant '{tenantIdentifier}' không tồn tại.",
                    code = "TENANT_NOT_FOUND"
                });
                return;
            }

            // Check tenant status
            if (tenant.Status == TenantStatus.Suspended)
            {
                _logger.LogWarning("🔒 Tenant suspended: {TenantId}", tenantIdentifier);
                
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Subscription expired",
                    message = "Tài khoản đã bị tạm khóa do chưa thanh toán. Vui lòng gia hạn để tiếp tục sử dụng.",
                    code = "TENANT_SUSPENDED",
                    renewUrl = "https://novasaas.com/renew"
                });
                return;
            }

            if (tenant.Status == TenantStatus.Terminated)
            {
                _logger.LogWarning("❌ Tenant terminated: {TenantId}", tenantIdentifier);
                
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Tenant terminated",
                    message = "Tài khoản đã bị hủy vĩnh viễn. Vui lòng liên hệ hỗ trợ.",
                    code = "TENANT_TERMINATED"
                });
                return;
            }

            if (tenant.Status != TenantStatus.Active)
            {
                _logger.LogWarning("⏳ Tenant not active: {TenantId} (Status: {Status})", tenantIdentifier, tenant.Status);
                
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Tenant not ready",
                    message = "Hệ thống đang được khởi tạo. Vui lòng thử lại sau ít phút.",
                    code = "TENANT_PROVISIONING"
                });
                return;
            }

            // ========================================
            // BƯỚC 4: THIẾT LẬP NGỮ CẢNH (Context Injection)
            // ========================================
            tenantService.SetTenant(tenant.Id, tenant.SchemaName);
            
            // Lưu thông tin tenant vào HttpContext để các component khác có thể truy cập
            context.Items["TenantId"] = tenant.Id;
            context.Items["TenantName"] = tenant.Name;
            context.Items["SchemaName"] = tenant.SchemaName;
            context.Items["PlanId"] = tenant.PlanId;

            _logger.LogInformation("✅ Tenant resolved: {TenantName} ({Schema}) - Plan: {PlanId}", 
                tenant.Name, tenant.SchemaName, tenant.PlanId);

            // Tiếp tục pipeline
            await _next(context);
        }

        #region Bước 2: Extract Tenant Identifier

        /// <summary>
        /// Trích xuất Tenant Identifier từ nhiều nguồn theo thứ tự ưu tiên:
        /// 1. Custom Header (X-Tenant-Id)
        /// 2. JWT Claim (tenant_id)
        /// 3. Query String (?tenant=xxx) - Chỉ cho dev/testing
        /// 4. Subdomain (apple.novasaas.com → "apple")
        /// </summary>
        private string? ExtractTenantIdentifier(HttpContext context)
        {
            // Nguồn 1: Custom Header (Ưu tiên cao nhất)
            if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
            {
                var tenantFromHeader = headerValue.ToString().Trim();
                if (!string.IsNullOrEmpty(tenantFromHeader))
                {
                    _logger.LogDebug("📨 Tenant from Header: {Tenant}", tenantFromHeader);
                    return tenantFromHeader;
                }
            }

            // Nguồn 2: JWT Claim (Nếu đã đăng nhập)
            var tenantClaim = context.User?.FindFirst("tenant_id")?.Value 
                           ?? context.User?.FindFirst("tenant")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim))
            {
                _logger.LogDebug("🔐 Tenant from JWT: {Tenant}", tenantClaim);
                return tenantClaim;
            }

            // Nguồn 3: Query String (Chỉ khi Development)
            if (context.Request.Query.TryGetValue("tenant", out var queryValue))
            {
                var tenantFromQuery = queryValue.ToString().Trim();
                if (!string.IsNullOrEmpty(tenantFromQuery))
                {
                    _logger.LogDebug("❓ Tenant from Query: {Tenant}", tenantFromQuery);
                    return tenantFromQuery;
                }
            }

            // Nguồn 4: Subdomain
            var host = context.Request.Host.Host;
            var tenantFromSubdomain = ExtractFromSubdomain(host);
            if (!string.IsNullOrEmpty(tenantFromSubdomain))
            {
                _logger.LogDebug("🌐 Tenant from Subdomain: {Tenant}", tenantFromSubdomain);
                return tenantFromSubdomain;
            }

            return null;
        }

        /// <summary>
        /// Trích xuất tenant từ subdomain.
        /// Ví dụ: apple.novasaas.com → "apple"
        /// </summary>
        private string? ExtractFromSubdomain(string host)
        {
            // Bỏ qua localhost và IP addresses
            if (host == "localhost" || 
                host.StartsWith("127.") || 
                host.StartsWith("192.168.") ||
                host.StartsWith("10.") ||
                IsIpAddress(host))
            {
                return null;
            }

            var segments = host.Split('.');
            
            // Yêu cầu ít nhất 3 phần: subdomain.domain.tld
            // Ví dụ: apple.novasaas.com → ["apple", "novasaas", "com"]
            if (segments.Length >= 3)
            {
                var subdomain = segments[0].ToLowerInvariant();
                
                // Bỏ qua www và api
                if (subdomain != "www" && subdomain != "api")
                {
                    return subdomain;
                }
            }

            return null;
        }

        private static bool IsIpAddress(string host)
        {
            return System.Net.IPAddress.TryParse(host, out _);
        }

        #endregion

        #region Bước 3: Validate Tenant

        /// <summary>
        /// Xác thực Tenant có tồn tại và đang hoạt động.
        /// Sử dụng IMemoryCache để tối ưu performance.
        /// </summary>
        private async Task<Tenant?> ValidateTenantAsync(
            string tenantIdentifier, 
            ApplicationDbContext dbContext,
            IMemoryCache cache)
        {
            // Cache key dựa trên tenant identifier
            var cacheKey = $"tenant:{tenantIdentifier.ToLowerInvariant()}";

            // Thử lấy từ cache trước
            if (cache.TryGetValue(cacheKey, out Tenant? cachedTenant))
            {
                _logger.LogDebug("📦 Tenant found in cache: {Tenant}", tenantIdentifier);
                return cachedTenant;
            }

            // Query database
            // Lưu ý: Bảng Tenants luôn ở schema 'public'
            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .Include(t => t.Plan)
                .FirstOrDefaultAsync(t => 
                    (t.Subdomain.ToLower() == tenantIdentifier.ToLower() || 
                     t.SchemaName.ToLower() == tenantIdentifier.ToLower() ||
                     t.Id.ToString().ToLower() == tenantIdentifier.ToLower()));

            if (tenant != null)
            {
                // Cache kết quả trong 5 phút
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                
                cache.Set(cacheKey, tenant, cacheOptions);
                _logger.LogDebug("💾 Tenant cached: {Tenant}", tenant.Name);
            }

            return tenant;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Kiểm tra xem path có nên bypass tenant check không.
        /// </summary>
        private static bool ShouldBypass(string path)
        {
            return BypassPaths.Any(bp => path.StartsWith(bp, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }

    /// <summary>
    /// Extension methods để đăng ký TenantMiddleware.
    /// </summary>
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}
