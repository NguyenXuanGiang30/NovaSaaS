using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.SCM
{
    /// <summary>
    /// GoodsReceipt - Phiếu nhận hàng từ nhà cung cấp.
    /// </summary>
    public class GoodsReceipt : BaseEntity
    {
        /// <summary>
        /// Mã phiếu nhận hàng.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ReceiptNumber { get; set; } = string.Empty;

        /// <summary>
        /// ID đơn đặt hàng (PO).
        /// </summary>
        public Guid PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

        /// <summary>
        /// ID kho nhận hàng.
        /// </summary>
        public Guid WarehouseId { get; set; }

        /// <summary>
        /// Ngày nhận hàng.
        /// </summary>
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft;

        /// <summary>
        /// Người nhận hàng.
        /// </summary>
        public Guid? ReceivedByUserId { get; set; }

        /// <summary>
        /// Số vận đơn.
        /// </summary>
        [MaxLength(50)]
        public string? DeliveryNoteNumber { get; set; }

        /// <summary>
        /// Ghi chú kiểm tra chất lượng.
        /// </summary>
        [MaxLength(2000)]
        public string? QualityNotes { get; set; }

        /// <summary>
        /// Ghi chú chung.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
    }

    /// <summary>
    /// GoodsReceiptItem - Chi tiết phiếu nhận hàng.
    /// </summary>
    public class GoodsReceiptItem : BaseEntity
    {
        /// <summary>
        /// ID phiếu nhận hàng.
        /// </summary>
        public Guid GoodsReceiptId { get; set; }
        public virtual GoodsReceipt GoodsReceipt { get; set; } = null!;

        /// <summary>
        /// ID sản phẩm.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// ID dòng PO.
        /// </summary>
        public Guid? PurchaseOrderItemId { get; set; }

        /// <summary>
        /// Số lượng đặt hàng.
        /// </summary>
        public int OrderedQuantity { get; set; }

        /// <summary>
        /// Số lượng nhận.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int ReceivedQuantity { get; set; }

        /// <summary>
        /// Số lượng chấp nhận (qua QC).
        /// </summary>
        [Range(0, int.MaxValue)]
        public int AcceptedQuantity { get; set; }

        /// <summary>
        /// Số lượng từ chối.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int RejectedQuantity { get; set; } = 0;

        /// <summary>
        /// Mã lô hàng (nếu có).
        /// </summary>
        [MaxLength(50)]
        public string? LotNumber { get; set; }

        /// <summary>
        /// Ngày hết hạn lô.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
