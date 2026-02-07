using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// LoyaltyProgram - Chương trình khách hàng thân thiết.
    /// </summary>
    public class LoyaltyProgram : BaseEntity
    {
        /// <summary>
        /// Tên chương trình.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chương trình.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Tỷ lệ tích điểm: bao nhiêu VND = 1 điểm.
        /// Ví dụ: 10000 nghĩa là chi 10,000 VND được 1 điểm.
        /// </summary>
        [Range(1, double.MaxValue)]
        public decimal PointsPerAmount { get; set; } = 10000;

        /// <summary>
        /// Giá trị quy đổi: 1 điểm = bao nhiêu VND.
        /// Ví dụ: 100 nghĩa là 1 điểm = 100 VND.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal PointValue { get; set; } = 100;

        /// <summary>
        /// Số điểm tối thiểu để đổi.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int MinRedeemPoints { get; set; } = 100;

        /// <summary>
        /// Ngày bắt đầu.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc (null = không giới hạn).
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
    }
}
