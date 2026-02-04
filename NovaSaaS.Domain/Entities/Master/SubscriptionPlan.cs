using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Master
{
    /// <summary>
    /// SubscriptionPlan - Định nghĩa các gói cước (Basic, Pro, Enterprise).
    /// </summary>
    public class SubscriptionPlan : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // Basic, Pro, Enterprise

        /// <summary>
        /// Mô tả gói cước.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        // ========================================
        // Pricing
        // ========================================

        /// <summary>
        /// Giá hàng tháng (VND hoặc USD tùy cấu hình).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal MonthlyPrice { get; set; }

        /// <summary>
        /// Giá hàng năm (thường discount 10-20%).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? YearlyPrice { get; set; }

        // ========================================
        // User Limits
        // ========================================

        /// <summary>
        /// Số lượng users tối đa trong tenant.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int MaxUsers { get; set; }

        // ========================================
        // AI & Usage Quotas
        // ========================================

        /// <summary>
        /// Số lượt gọi AI tối đa mỗi tháng.
        /// </summary>
        public int MaxMonthlyAICalls { get; set; } = 100;

        /// <summary>
        /// Dung lượng storage tối đa (MB).
        /// </summary>
        public int MaxStorageMB { get; set; } = 100;

        /// <summary>
        /// Số lượng documents tối đa cho RAG.
        /// </summary>
        public int MaxDocuments { get; set; } = 50;

        /// <summary>
        /// Có bật tính năng AI không.
        /// </summary>
        public bool AIEnabled { get; set; } = false;

        // ========================================
        // Billing Settings
        // ========================================

        /// <summary>
        /// Số ngày ân hạn sau khi hết hạn trước khi suspend.
        /// </summary>
        public int GracePeriodDays { get; set; } = 7;

        /// <summary>
        /// Có cho phép vượt quota không (tính phí thêm).
        /// </summary>
        public bool AllowOverage { get; set; } = false;

        /// <summary>
        /// Phí overage cho mỗi 1000 AI calls khi vượt quota.
        /// </summary>
        public decimal? OveragePricePer1000Calls { get; set; }

        // ========================================
        // Navigation Properties
        // ========================================

        public virtual ICollection<PlanFeature> Features { get; set; } = new List<PlanFeature>();
        public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
    }
}

