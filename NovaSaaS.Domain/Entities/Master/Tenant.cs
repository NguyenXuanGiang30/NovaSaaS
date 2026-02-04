using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Master
{
    /// <summary>
    /// Tenant - Đại diện cho một khách hàng (doanh nghiệp) trong hệ thống Multi-tenant.
    /// Lưu trữ trong schema 'public'.
    /// </summary>
    public class Tenant : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Subdomain { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string SchemaName { get; set; } = string.Empty;
        
        /// <summary>
        /// Trạng thái tenant: Provisioning, Active, Suspended, Terminated.
        /// </summary>
        public TenantStatus Status { get; set; } = TenantStatus.Provisioning;

        // ========================================
        // Subscription Info
        // ========================================
        
        public Guid PlanId { get; set; }
        public virtual SubscriptionPlan Plan { get; set; } = null!;

        /// <summary>
        /// Ngày bắt đầu subscription.
        /// </summary>
        public DateTime? SubscriptionStartDate { get; set; }

        /// <summary>
        /// Ngày hết hạn subscription.
        /// </summary>
        public DateTime? SubscriptionEndDate { get; set; }

        /// <summary>
        /// Ngày thanh toán gần nhất.
        /// </summary>
        public DateTime? LastBillingDate { get; set; }

        /// <summary>
        /// Lý do nếu bị suspend.
        /// </summary>
        [MaxLength(500)]
        public string? SuspendReason { get; set; }

        // ========================================
        // Usage Limits (Override từ Plan nếu cần)
        // ========================================

        /// <summary>
        /// Giới hạn số lượt gọi AI/tháng (null = dùng mặc định từ Plan).
        /// </summary>
        public int? MaxMonthlyAICalls { get; set; }

        /// <summary>
        /// Giới hạn dung lượng storage MB (null = dùng mặc định từ Plan).
        /// </summary>
        public int? MaxStorageMB { get; set; }

        // ========================================
        // Navigation Properties
        // ========================================

        public virtual ICollection<UsageLog> UsageLogs { get; set; } = new List<UsageLog>();
        public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();

        // ========================================
        // Computed Properties
        // ========================================

        /// <summary>
        /// Kiểm tra subscription có còn hiệu lực không.
        /// </summary>
        public bool IsSubscriptionValid => 
            SubscriptionEndDate.HasValue && SubscriptionEndDate.Value > DateTime.UtcNow;

        /// <summary>
        /// Số ngày còn lại của subscription.
        /// </summary>
        public int? DaysRemaining => 
            SubscriptionEndDate.HasValue 
                ? (int)(SubscriptionEndDate.Value - DateTime.UtcNow).TotalDays 
                : null;
    }
}

