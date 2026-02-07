using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.SCM
{
    /// <summary>
    /// PurchaseRequisition - Yêu cầu mua hàng (PR).
    /// </summary>
    public class PurchaseRequisition : BaseEntity
    {
        /// <summary>
        /// Mã yêu cầu mua hàng.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string RequisitionNumber { get; set; } = string.Empty;

        /// <summary>
        /// Ngày yêu cầu.
        /// </summary>
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày cần hàng.
        /// </summary>
        public DateTime? RequiredDate { get; set; }

        /// <summary>
        /// ID người yêu cầu.
        /// </summary>
        public Guid RequestedByUserId { get; set; }

        /// <summary>
        /// ID phòng ban yêu cầu.
        /// </summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public PurchaseRequisitionStatus Status { get; set; } = PurchaseRequisitionStatus.Draft;

        /// <summary>
        /// Lý do mua.
        /// </summary>
        [MaxLength(1000)]
        public string? Reason { get; set; }

        /// <summary>
        /// Tổng giá trị ước tính.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal EstimatedTotal { get; set; } = 0;

        /// <summary>
        /// Người duyệt.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Ngày duyệt.
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<PurchaseRequisitionItem> Items { get; set; } = new List<PurchaseRequisitionItem>();
    }

    /// <summary>
    /// PurchaseRequisitionItem - Chi tiết yêu cầu mua hàng.
    /// </summary>
    public class PurchaseRequisitionItem : BaseEntity
    {
        /// <summary>
        /// ID yêu cầu mua hàng.
        /// </summary>
        public Guid PurchaseRequisitionId { get; set; }
        public virtual PurchaseRequisition PurchaseRequisition { get; set; } = null!;

        /// <summary>
        /// ID sản phẩm.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Số lượng cần mua.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Đơn giá ước tính.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal EstimatedUnitPrice { get; set; } = 0;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
