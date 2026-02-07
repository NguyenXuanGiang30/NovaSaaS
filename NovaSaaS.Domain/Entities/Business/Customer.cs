using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Customer - Khách hàng trong hệ thống CRM.
    /// </summary>
    public class Customer : BaseEntity
    {
        /// <summary>
        /// Tên khách hàng / Tên công ty.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại.
        /// </summary>
        [MaxLength(20)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Mã số thuế.
        /// </summary>
        [MaxLength(20)]
        public string TaxCode { get; set; } = string.Empty;

        /// <summary>
        /// Hạng khách hàng (VIP, Gold, Standard...).
        /// </summary>
        public CustomerRank Rank { get; set; } = CustomerRank.Standard;

        /// <summary>
        /// Loại khách hàng (Cá nhân / Doanh nghiệp).
        /// </summary>
        public CustomerType Type { get; set; } = CustomerType.Retail;

        /// <summary>
        /// Tổng chi tiêu tích lũy.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalSpending { get; set; } = 0;

        /// <summary>
        /// Địa chỉ.
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Email.
        /// </summary>
        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Website.
        /// </summary>
        [MaxLength(500)]
        public string? Website { get; set; }

        /// <summary>
        /// Ngành nghề.
        /// </summary>
        [MaxLength(100)]
        public string? Industry { get; set; }

        /// <summary>
        /// Hạn mức tín dụng (công nợ tối đa cho phép).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CreditLimit { get; set; } = 0;

        /// <summary>
        /// Công nợ hiện tại.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CurrentDebt { get; set; } = 0;

        /// <summary>
        /// ID nhân viên Sales phụ trách.
        /// </summary>
        public Guid? AssignedSalesUserId { get; set; }

        /// <summary>
        /// Nhóm khách hàng (tùy chọn).
        /// </summary>
        public Guid? CustomerGroupId { get; set; }
        public virtual CustomerGroup? CustomerGroup { get; set; }

        /// <summary>
        /// Tổng điểm loyalty hiện tại.
        /// </summary>
        public int LoyaltyPoints { get; set; } = 0;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
    }
}
