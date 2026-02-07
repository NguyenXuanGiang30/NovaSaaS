using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.HRM
{
    /// <summary>
    /// EmployeeContract - Hợp đồng lao động.
    /// </summary>
    public class EmployeeContract : BaseEntity
    {
        /// <summary>
        /// Mã hợp đồng.
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string ContractNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID nhân viên.
        /// </summary>
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        /// <summary>
        /// Loại hợp đồng.
        /// </summary>
        public ContractType ContractType { get; set; } = ContractType.Permanent;

        /// <summary>
        /// Ngày bắt đầu hợp đồng.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc hợp đồng (null = vô thời hạn).
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Lương cơ bản theo hợp đồng.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal BaseSalary { get; set; }

        /// <summary>
        /// Phụ cấp cố định.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Allowance { get; set; } = 0;

        /// <summary>
        /// Ngày ký hợp đồng.
        /// </summary>
        public DateTime SignedDate { get; set; }

        /// <summary>
        /// Người ký (đại diện công ty).
        /// </summary>
        [MaxLength(100)]
        public string? SignedBy { get; set; }

        /// <summary>
        /// Hợp đồng hiện đang hiệu lực.
        /// </summary>
        public bool IsCurrent { get; set; } = true;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// File đính kèm hợp đồng (URL).
        /// </summary>
        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }
    }
}
