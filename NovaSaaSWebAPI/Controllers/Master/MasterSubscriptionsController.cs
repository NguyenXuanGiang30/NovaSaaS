using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.Services.Master;
using System;
using System.Threading.Tasks;

namespace NovaSaaS.WebApi.Controllers.Master
{
    /// <summary>
    /// MasterSubscriptionsController - Quản lý Subscriptions cho Master Admin.
    /// </summary>
    [ApiController]
    [Route("api/master/subscriptions")]
    [Authorize(Roles = "MasterAdmin")]
    public class MasterSubscriptionsController : ControllerBase
    {
        private readonly SubscriptionService _subscriptionService;

        public MasterSubscriptionsController(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Lấy danh sách tất cả subscriptions.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var subscriptions = await _subscriptionService.GetAllSubscriptionsAsync();
            return Ok(subscriptions);
        }

        /// <summary>
        /// Gia hạn subscription cho Tenant.
        /// </summary>
        [HttpPut("{tenantId:guid}/renew")]
        public async Task<IActionResult> Renew(Guid tenantId, [FromBody] RenewRequest request)
        {
            if (request.Months < 1 || request.Months > 24)
                return BadRequest(new { error = "Số tháng gia hạn phải từ 1-24." });

            var result = await _subscriptionService.RenewSubscriptionAsync(tenantId, request.Months);
            
            if (!result.Success)
                return BadRequest(new { error = result.Message, code = result.ErrorCode });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Nâng cấp gói cho Tenant.
        /// </summary>
        [HttpPut("{tenantId:guid}/upgrade")]
        public async Task<IActionResult> Upgrade(Guid tenantId, [FromBody] UpgradeRequest request)
        {
            var result = await _subscriptionService.UpgradePlanAsync(tenantId, request.NewPlanId);
            
            if (!result.Success)
                return BadRequest(new { error = result.Message, code = result.ErrorCode });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Lấy danh sách Tenants sắp hết hạn.
        /// </summary>
        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiring([FromQuery] int days = 7)
        {
            if (days < 1 || days > 90)
                days = 7;

            var expiring = await _subscriptionService.GetExpiringTenantsAsync(days);
            return Ok(new
            {
                count = expiring.Count,
                daysThreshold = days,
                tenants = expiring
            });
        }
    }

    public class RenewRequest
    {
        public int Months { get; set; } = 1;
    }

    public class UpgradeRequest
    {
        public Guid NewPlanId { get; set; }
    }
}
