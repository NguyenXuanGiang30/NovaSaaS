using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace NovaSaaS.Application.Jobs
{
    /// <summary>
    /// SubscriptionCheckJob - Job kiểm tra và khóa các tenant hết hạn subscription.
    /// Chạy hàng ngày vào 00:00 UTC.
    /// </summary>
    public class SubscriptionCheckJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SubscriptionCheckJob> _logger;

        public SubscriptionCheckJob(IUnitOfWork unitOfWork, ILogger<SubscriptionCheckJob> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Thực thi job kiểm tra subscription.
        /// </summary>
        public async Task ExecuteAsync()
        {
            _logger.LogInformation("🔍 Starting SubscriptionCheckJob...");
            
            var now = DateTime.UtcNow;
            var expiredCount = 0;
            var gracePeriodCount = 0;

            try
            {
                // Lấy tất cả tenants đang Active
                var activeTenants = await _unitOfWork.Tenants.GetAllAsync(
                    t => t.Status == TenantStatus.Active && 
                         t.SubscriptionEndDate.HasValue);

                foreach (var tenant in activeTenants)
                {
                    if (!tenant.SubscriptionEndDate.HasValue) continue;

                    var endDate = tenant.SubscriptionEndDate.Value;
                    var gracePeriodDays = tenant.Plan?.GracePeriodDays ?? 7;
                    var graceEndDate = endDate.AddDays(gracePeriodDays);

                    // Đã hết grace period → Suspend
                    if (now > graceEndDate)
                    {
                        tenant.Status = TenantStatus.Suspended;
                        tenant.SuspendReason = "Subscription hết hạn và đã qua grace period.";
                        tenant.UpdateAt = now;
                        
                        _unitOfWork.Tenants.Update(tenant);
                        expiredCount++;
                        
                        _logger.LogWarning("⛔ Tenant suspended: {TenantName} (expired: {ExpireDate})", 
                            tenant.Name, endDate);
                    }
                    // Đang trong grace period → Log cảnh báo
                    else if (now > endDate)
                    {
                        gracePeriodCount++;
                        var daysLeft = (graceEndDate - now).Days;
                        
                        _logger.LogWarning("⚠️ Tenant in grace period: {TenantName} ({DaysLeft} days left)", 
                            tenant.Name, daysLeft);
                        
                        // TODO: Gửi email nhắc nhở khi có email service
                    }
                }

                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "✅ SubscriptionCheckJob completed. Suspended: {Suspended}, In Grace Period: {Grace}", 
                    expiredCount, gracePeriodCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ SubscriptionCheckJob failed");
                throw;
            }
        }
    }
}
