using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Lead - Khách hàng tiềm năng trong pipeline bán hàng.
    /// </summary>
    public class Lead : BaseEntity
    {
        /// <summary>
        /// Họ tên liên hệ.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ContactName { get; set; } = string.Empty;

        /// <summary>
        /// Tên công ty.
        /// </summary>
        [MaxLength(200)]
        public string? CompanyName { get; set; }

        /// <summary>
        /// Email.
        /// </summary>
        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Số điện thoại.
        /// </summary>
        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        /// <summary>
        /// Nguồn Lead.
        /// </summary>
        public LeadSource Source { get; set; } = LeadSource.Other;

        /// <summary>
        /// Trạng thái Lead.
        /// </summary>
        public LeadStatus Status { get; set; } = LeadStatus.New;

        /// <summary>
        /// Giá trị ước tính (VND).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal EstimatedValue { get; set; } = 0;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Người được giao phụ trách.
        /// </summary>
        public Guid? AssignedToUserId { get; set; }

        /// <summary>
        /// ID Customer nếu đã chuyển đổi.
        /// </summary>
        public Guid? ConvertedCustomerId { get; set; }
        public virtual Customer? ConvertedCustomer { get; set; }

        /// <summary>
        /// Ngày chuyển đổi.
        /// </summary>
        public DateTime? ConvertedAt { get; set; }

        // Navigation
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }
}
