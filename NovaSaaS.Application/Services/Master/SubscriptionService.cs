using Microsoft.Extensions.Logging;
using NovaSaaS.Domain.Entities.Master;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Master
{
    /// <summary>
    /// SubscriptionService - Quản lý thuê bao của Tenants.
    /// Bao gồm: Gia hạn, Nâng cấp, Tạm khóa, Kích hoạt lại.
    /// </summary>
    public class SubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(IUnitOfWork unitOfWork, ILogger<SubscriptionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        #region Subscription Management

        /// <summary>
        /// Lấy thông tin subscription của một Tenant.
        /// </summary>
        public async Task<TenantSubscriptionDto?> GetSubscriptionAsync(Guid tenantId)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null) return null;

            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(tenant.PlanId);

            return new TenantSubscriptionDto
            {
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                PlanId = tenant.PlanId,
                PlanName = plan?.Name ?? "Unknown",
                Status = tenant.Status,
                SubscriptionStartDate = tenant.SubscriptionStartDate,
                SubscriptionEndDate = tenant.SubscriptionEndDate,
                LastBillingDate = tenant.LastBillingDate,
                DaysRemaining = tenant.DaysRemaining,
                IsSubscriptionValid = tenant.IsSubscriptionValid,
                MaxMonthlyAICalls = tenant.MaxMonthlyAICalls ?? plan?.MaxMonthlyAICalls ?? 0,
                MaxStorageMB = tenant.MaxStorageMB ?? plan?.MaxStorageMB ?? 0,
                AIEnabled = plan?.AIEnabled ?? false
            };
        }

        /// <summary>
        /// Gia hạn subscription cho Tenant.
        /// </summary>
        public async Task<SubscriptionResult> RenewSubscriptionAsync(Guid tenantId, int months)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                return SubscriptionResult.Fail("Tenant không tồn tại.", "TENANT_NOT_FOUND");

            if (months < 1 || months > 24)
                return SubscriptionResult.Fail("Số tháng gia hạn phải từ 1-24.", "INVALID_MONTHS");

            // Tính ngày hết hạn mới
            var baseDate = tenant.SubscriptionEndDate.HasValue && tenant.SubscriptionEndDate.Value > DateTime.UtcNow
                ? tenant.SubscriptionEndDate.Value
                : DateTime.UtcNow;

            tenant.SubscriptionEndDate = baseDate.AddMonths(months);
            tenant.LastBillingDate = DateTime.UtcNow;
            tenant.UpdateAt = DateTime.UtcNow;

            // Nếu tenant đang bị suspend do hết hạn, kích hoạt lại
            if (tenant.Status == TenantStatus.Suspended)
            {
                tenant.Status = TenantStatus.Active;
                tenant.SuspendReason = null;
                _logger.LogInformation("🔓 Tenant {TenantName} đã được kích hoạt lại sau gia hạn", tenant.Name);
            }

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("💳 Gia hạn {Months} tháng cho Tenant {TenantName}. Hết hạn: {EndDate}", 
                months, tenant.Name, tenant.SubscriptionEndDate);

            return SubscriptionResult.Ok($"Đã gia hạn {months} tháng. Subscription đến: {tenant.SubscriptionEndDate:dd/MM/yyyy}");
        }

        /// <summary>
        /// Nâng cấp gói cho Tenant.
        /// </summary>
        public async Task<SubscriptionResult> UpgradePlanAsync(Guid tenantId, Guid newPlanId)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                return SubscriptionResult.Fail("Tenant không tồn tại.", "TENANT_NOT_FOUND");

            if (tenant.PlanId == newPlanId)
                return SubscriptionResult.Fail("Tenant đã sử dụng gói này.", "SAME_PLAN");

            var newPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(newPlanId);
            if (newPlan == null)
                return SubscriptionResult.Fail("Gói cước không tồn tại.", "PLAN_NOT_FOUND");

            var oldPlanId = tenant.PlanId;
            tenant.PlanId = newPlanId;
            tenant.UpdateAt = DateTime.UtcNow;

            // Reset custom limits khi chuyển plan
            tenant.MaxMonthlyAICalls = null;
            tenant.MaxStorageMB = null;

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("📈 Tenant {TenantName} đã nâng cấp từ Plan {OldPlan} lên {NewPlan}", 
                tenant.Name, oldPlanId, newPlan.Name);

            return SubscriptionResult.Ok($"Đã nâng cấp lên gói {newPlan.Name}.");
        }

        /// <summary>
        /// Tạm khóa Tenant.
        /// </summary>
        public async Task<SubscriptionResult> SuspendTenantAsync(Guid tenantId, string reason)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                return SubscriptionResult.Fail("Tenant không tồn tại.", "TENANT_NOT_FOUND");

            if (tenant.Status == TenantStatus.Terminated)
                return SubscriptionResult.Fail("Tenant đã bị hủy vĩnh viễn.", "TENANT_TERMINATED");

            if (tenant.Status == TenantStatus.Suspended)
                return SubscriptionResult.Fail("Tenant đã bị tạm khóa.", "ALREADY_SUSPENDED");

            tenant.Status = TenantStatus.Suspended;
            tenant.SuspendReason = reason;
            tenant.UpdateAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            _logger.LogWarning("🔒 Tenant {TenantName} đã bị tạm khóa. Lý do: {Reason}", tenant.Name, reason);

            return SubscriptionResult.Ok($"Đã tạm khóa Tenant. Lý do: {reason}");
        }

        /// <summary>
        /// Kích hoạt lại Tenant đã bị tạm khóa.
        /// </summary>
        public async Task<SubscriptionResult> ReactivateTenantAsync(Guid tenantId)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                return SubscriptionResult.Fail("Tenant không tồn tại.", "TENANT_NOT_FOUND");

            if (tenant.Status == TenantStatus.Terminated)
                return SubscriptionResult.Fail("Tenant đã bị hủy vĩnh viễn và không thể kích hoạt lại.", "TENANT_TERMINATED");

            if (tenant.Status == TenantStatus.Active)
                return SubscriptionResult.Fail("Tenant đang hoạt động.", "ALREADY_ACTIVE");

            tenant.Status = TenantStatus.Active;
            tenant.SuspendReason = null;
            tenant.UpdateAt = DateTime.UtcNow;

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("🔓 Tenant {TenantName} đã được kích hoạt lại", tenant.Name);

            return SubscriptionResult.Ok("Đã kích hoạt lại Tenant.");
        }

        #endregion

        #region Queries

        /// <summary>
        /// Lấy danh sách Tenants sắp hết hạn.
        /// </summary>
        public async Task<IList<TenantExpiryDto>> GetExpiringTenantsAsync(int daysUntilExpiry)
        {
            var threshold = DateTime.UtcNow.AddDays(daysUntilExpiry);

            var tenants = await _unitOfWork.Tenants.FindAsync(
                t => t.Status == TenantStatus.Active && 
                     t.SubscriptionEndDate.HasValue && 
                     t.SubscriptionEndDate.Value <= threshold,
                t => t.Plan);

            return tenants
                .OrderBy(t => t.SubscriptionEndDate)
                .Select(t => new TenantExpiryDto
                {
                    TenantId = t.Id,
                    TenantName = t.Name,
                    PlanName = t.Plan?.Name ?? "Unknown",
                    SubscriptionEndDate = t.SubscriptionEndDate!.Value,
                    DaysRemaining = t.DaysRemaining ?? 0
                })
                .ToList();
        }

        /// <summary>
        /// Lấy tất cả subscriptions (cho Master Admin).
        /// </summary>
        public async Task<IList<TenantSubscriptionDto>> GetAllSubscriptionsAsync()
        {
            var tenants = await _unitOfWork.Tenants.FindAsync(_ => true, t => t.Plan);

            return tenants
                .OrderByDescending(t => t.CreateAt)
                .Select(t => new TenantSubscriptionDto
                {
                    TenantId = t.Id,
                    TenantName = t.Name,
                    PlanId = t.PlanId,
                    PlanName = t.Plan?.Name ?? "Unknown",
                    Status = t.Status,
                    SubscriptionStartDate = t.SubscriptionStartDate,
                    SubscriptionEndDate = t.SubscriptionEndDate,
                    LastBillingDate = t.LastBillingDate,
                    DaysRemaining = t.DaysRemaining,
                    IsSubscriptionValid = t.IsSubscriptionValid,
                    MaxMonthlyAICalls = t.MaxMonthlyAICalls ?? t.Plan?.MaxMonthlyAICalls ?? 0,
                    MaxStorageMB = t.MaxStorageMB ?? t.Plan?.MaxStorageMB ?? 0,
                    AIEnabled = t.Plan?.AIEnabled ?? false
                })
                .ToList();
        }

        /// <summary>
        /// Tìm các Tenants cần bị suspend do quá hạn.
        /// </summary>
        public async Task<IList<Tenant>> GetTenantsToSuspendAsync()
        {
            var plans = await _unitOfWork.SubscriptionPlans.GetAllAsync();
            var planDict = plans.ToDictionary(p => p.Id, p => p.GracePeriodDays);

            var allActiveTenants = await _unitOfWork.Tenants.FindAsync(
                t => t.Status == TenantStatus.Active && t.SubscriptionEndDate.HasValue);

            var now = DateTime.UtcNow;
            var tenantsToSuspend = allActiveTenants
                .Where(t =>
                {
                    var gracePeriod = planDict.TryGetValue(t.PlanId, out var gp) ? gp : 7;
                    return t.SubscriptionEndDate!.Value.AddDays(gracePeriod) < now;
                })
                .ToList();

            return tenantsToSuspend;
        }

        #endregion
    }

    #region DTOs

    public class TenantSubscriptionDto
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public TenantStatus Status { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public DateTime? LastBillingDate { get; set; }
        public int? DaysRemaining { get; set; }
        public bool IsSubscriptionValid { get; set; }
        public int MaxMonthlyAICalls { get; set; }
        public int MaxStorageMB { get; set; }
        public bool AIEnabled { get; set; }
    }

    public class TenantExpiryDto
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public DateTime SubscriptionEndDate { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class SubscriptionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }

        public static SubscriptionResult Ok(string message) => new() { Success = true, Message = message };
        public static SubscriptionResult Fail(string message, string errorCode) => 
            new() { Success = false, Message = message, ErrorCode = errorCode };
    }

    #endregion
}
