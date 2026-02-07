using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.SCM
{
    /// <summary>
    /// ReturnToVendor - Trả hàng cho nhà cung cấp.
    /// </summary>
    public class ReturnToVendor : BaseEntity
    {
        /// <summary>
        /// Mã phiếu trả hàng.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ReturnNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID nhà cung cấp.
        /// </summary>
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; } = null!;

        /// <summary>
        /// ID đơn đặt hàng gốc.
        /// </summary>
        public Guid? PurchaseOrderId { get; set; }
        public virtual PurchaseOrder? PurchaseOrder { get; set; }

        /// <summary>
        /// ID phiếu nhận hàng gốc.
        /// </summary>
        public Guid? GoodsReceiptId { get; set; }

        /// <summary>
        /// Ngày trả.
        /// </summary>
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Lý do trả.
        /// </summary>
        public ReturnReason Reason { get; set; } = ReturnReason.Defective;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public ReturnToVendorStatus Status { get; set; } = ReturnToVendorStatus.Requested;

        /// <summary>
        /// Tổng giá trị.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Số tiền hoàn trả.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal RefundAmount { get; set; } = 0;

        /// <summary>
        /// Ghi chú chi tiết.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<ReturnToVendorItem> Items { get; set; } = new List<ReturnToVendorItem>();
    }

    /// <summary>
    /// ReturnToVendorItem - Chi tiết trả hàng.
    /// </summary>
    public class ReturnToVendorItem : BaseEntity
    {
        /// <summary>
        /// ID phiếu trả hàng.
        /// </summary>
        public Guid ReturnToVendorId { get; set; }
        public virtual ReturnToVendor ReturnToVendor { get; set; } = null!;

        /// <summary>
        /// ID sản phẩm.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Số lượng trả.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Đơn giá.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Lý do trả cụ thể.
        /// </summary>
        [MaxLength(500)]
        public string? ReasonDetail { get; set; }
    }
}
