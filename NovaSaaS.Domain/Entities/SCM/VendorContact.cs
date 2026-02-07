using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.SCM
{
    /// <summary>
    /// VendorContact - Liên hệ của nhà cung cấp.
    /// </summary>
    public class VendorContact : BaseEntity
    {
        /// <summary>
        /// ID nhà cung cấp.
        /// </summary>
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; } = null!;

        /// <summary>
        /// Họ tên.
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
        /// Là liên hệ chính.
        /// </summary>
        public bool IsPrimary { get; set; } = false;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
