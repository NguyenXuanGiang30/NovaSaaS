using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Liên hệ - Quản lý nhiều người liên hệ cho mỗi khách hàng.
    /// </summary>
    public class Contact : BaseEntity
    {
        /// <summary>
        /// ID khách hàng.
        /// </summary>
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        /// <summary>
        /// Họ tên người liên hệ.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Chức vụ.
        /// </summary>
        [MaxLength(100)]
        public string? JobTitle { get; set; }

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
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Liên hệ chính (primary contact).
        /// </summary>
        public bool IsPrimary { get; set; } = false;
    }
}
