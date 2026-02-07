using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.SCM
{
    /// <summary>
    /// Vendor - Nhà cung cấp.
    /// </summary>
    public class Vendor : BaseEntity
    {
        /// <summary>
        /// Mã nhà cung cấp.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên nhà cung cấp.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mã số thuế.
        /// </summary>
        [MaxLength(20)]
        public string? TaxCode { get; set; }

        /// <summary>
        /// Địa chỉ.
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Số điện thoại.
        /// </summary>
        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

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
        /// Người liên hệ chính.
        /// </summary>
        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        /// <summary>
        /// Số tài khoản ngân hàng.
        /// </summary>
        [MaxLength(30)]
        public string? BankAccountNumber { get; set; }

        /// <summary>
        /// Tên ngân hàng.
        /// </summary>
        [MaxLength(100)]
        public string? BankName { get; set; }

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public VendorStatus Status { get; set; } = VendorStatus.Active;

        /// <summary>
        /// Đánh giá.
        /// </summary>
        public VendorRating Rating { get; set; } = VendorRating.Unrated;

        /// <summary>
        /// Thời gian giao hàng trung bình (ngày).
        /// </summary>
        public int? AverageLeadTimeDays { get; set; }

        /// <summary>
        /// Điều khoản thanh toán (số ngày).
        /// </summary>
        public int PaymentTermDays { get; set; } = 30;

        /// <summary>
        /// Hạn mức tín dụng.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CreditLimit { get; set; } = 0;

        /// <summary>
        /// Công nợ hiện tại.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CurrentDebt { get; set; } = 0;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<VendorContact> Contacts { get; set; } = new List<VendorContact>();
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public virtual ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
    }
}
