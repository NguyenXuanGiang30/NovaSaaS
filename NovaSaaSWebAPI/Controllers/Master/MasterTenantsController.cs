using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.Services.Master;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.WebApi.Controllers.Master
{
    /// <summary>
    /// MasterTenantsController - Quản lý Tenants cho Master Admin.
    /// Yêu cầu quyền "MasterAdmin".
    /// </summary>
    [ApiController]
    [Route("api/master/tenants")]
    [Authorize(Roles = "MasterAdmin")]
    public class MasterTenantsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SubscriptionService _subscriptionService;

        public MasterTenantsController(
            IUnitOfWork unitOfWork,
            SubscriptionService subscriptionService)
        {
            _unitOfWork = unitOfWork;
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Lấy danh sách tất cả Tenants.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TenantStatus? status = null)
        {
            var tenants = await _unitOfWork.Tenants.FindAsync(_ => true, t => t.Plan);
            
            if (status.HasValue)
                tenants = tenants.Where(t => t.Status == status.Value);

            var result = tenants.Select(t => new
            {
                t.Id,
                t.Name,
                t.Subdomain,
                t.SchemaName,
                t.Status,
                PlanName = t.Plan?.Name ?? "Unknown",
                t.SubscriptionEndDate,
                t.DaysRemaining,
                t.CreateAt
            }).OrderByDescending(t => t.CreateAt);

            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết một Tenant.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var subscription = await _subscriptionService.GetSubscriptionAsync(id);
            if (subscription == null)
                return NotFound(new { error = "Tenant không tồn tại." });

            return Ok(subscription);
        }

        /// <summary>
        /// Tạm khóa Tenant.
        /// </summary>
        [HttpPut("{id:guid}/suspend")]
        public async Task<IActionResult> Suspend(Guid id, [FromBody] SuspendRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest(new { error = "Vui lòng cung cấp lý do." });

            var result = await _subscriptionService.SuspendTenantAsync(id, request.Reason);
            
            if (!result.Success)
                return BadRequest(new { error = result.Message, code = result.ErrorCode });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Kích hoạt lại Tenant.
        /// </summary>
        [HttpPut("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            var result = await _subscriptionService.ReactivateTenantAsync(id);
            
            if (!result.Success)
                return BadRequest(new { error = result.Message, code = result.ErrorCode });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Soft delete Tenant.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(id);
            if (tenant == null)
                return NotFound(new { error = "Tenant không tồn tại." });

            tenant.Status = TenantStatus.Terminated;
            tenant.UpdateAt = DateTime.UtcNow;
            tenant.IsDeleted = true;
            tenant.DeletedAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            return Ok(new { message = "Đã hủy Tenant thành công." });
        }
    }

    public class SuspendRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
