using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.Services.Master;
using System;
using System.Threading.Tasks;

namespace NovaSaaS.WebApi.Controllers.Master
{
    /// <summary>
    /// MasterUsageController - Theo dõi Resource Usage cho Master Admin.
    /// </summary>
    [ApiController]
    [Route("api/master/usage")]
    [Authorize(Roles = "MasterAdmin")]
    public class MasterUsageController : ControllerBase
    {
        private readonly UsageTrackingService _usageService;

        public MasterUsageController(UsageTrackingService usageService)
        {
            _usageService = usageService;
        }

        /// <summary>
        /// Lấy usage overview của tất cả Tenants trong tháng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTenantsUsage([FromQuery] int? year, [FromQuery] int? month)
        {
            var now = DateTime.UtcNow;
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            var usage = await _usageService.GetAllTenantsUsageAsync(y, m);
            
            return Ok(new
            {
                period = $"{y}-{m:D2}",
                totalTenants = usage.Count,
                totalCostUSD = usage.Sum(u => u.TotalCostUSD),
                totalTokens = usage.Sum(u => u.TotalTokens),
                tenants = usage
            });
        }

        /// <summary>
        /// Lấy usage chi tiết của một Tenant.
        /// </summary>
        [HttpGet("{tenantId:guid}")]
        public async Task<IActionResult> GetTenantUsage(Guid tenantId, [FromQuery] int? year, [FromQuery] int? month)
        {
            var now = DateTime.UtcNow;
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            var usage = await _usageService.GetMonthlyUsageAsync(tenantId, y, m);
            return Ok(usage);
        }

        /// <summary>
        /// Kiểm tra quota status của một Tenant.
        /// </summary>
        [HttpGet("{tenantId:guid}/quota")]
        public async Task<IActionResult> GetQuotaStatus(Guid tenantId)
        {
            try
            {
                var quota = await _usageService.CheckAIQuotaAsync(tenantId);
                return Ok(quota);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thống kê AI costs tổng hợp.
        /// </summary>
        [HttpGet("ai-costs")]
        public async Task<IActionResult> GetAICosts([FromQuery] int? year, [FromQuery] int? month)
        {
            var now = DateTime.UtcNow;
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            var usage = await _usageService.GetAllTenantsUsageAsync(y, m);
            
            var totalCost = usage.Sum(u => u.TotalCostUSD);
            var totalTokens = usage.Sum(u => u.TotalTokens);
            var totalCalls = usage.Sum(u => u.TotalAICalls);

            return Ok(new
            {
                period = $"{y}-{m:D2}",
                totalCostUSD = Math.Round(totalCost, 4),
                totalTokens = totalTokens,
                totalAICalls = totalCalls,
                avgCostPerCall = totalCalls > 0 ? Math.Round(totalCost / totalCalls, 6) : 0,
                avgTokensPerCall = totalCalls > 0 ? totalTokens / totalCalls : 0,
                topConsumers = usage.Take(5).Select(u => new
                {
                    u.TenantId,
                    u.TenantName,
                    u.TotalCostUSD,
                    u.TotalTokens
                })
            });
        }
    }
}
