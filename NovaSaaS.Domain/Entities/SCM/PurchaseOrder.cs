using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.PM;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.SCM
{
    /// <summary>
    /// PurchaseOrder - Đơn đặt hàng mua (PO).
    /// </summary>
    public class PurchaseOrder : BaseEntity
    {
        /// <summary>
        /// Mã đơn đặt hàng.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PONumber { get; set; } = string.Empty;

        /// <summary>
        /// ID nhà cung cấp.
        /// </summary>
        public Guid VendorId { get; set; }
        public virtual Vendor Vendor { get; set; } = null!;

        /// <summary>
        /// Ngày đặt hàng.
        /// </summary>
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày giao hàng dự kiến.
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// ID kho nhận hàng.
        /// </summary>
        public Guid? WarehouseId { get; set; }

        /// <summary>
        /// Trạng thái.
        /// </summary>
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

        /// <summary>
        /// Tổng tiền hàng.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal SubTotal { get; set; }

        /// <summary>
        /// Thuế.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Giảm giá.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// Phí vận chuyển.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal ShippingFee { get; set; } = 0;

        /// <summary>
        /// Tổng thanh toán.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Điều khoản thanh toán.
        /// </summary>
        [MaxLength(200)]
        public string? PaymentTerms { get; set; }

        /// <summary>
        /// ID yêu cầu mua hàng gốc.
        /// </summary>
        public Guid? PurchaseRequisitionId { get; set; }

        /// <summary>
        /// Người tạo PO.
        /// </summary>
        public Guid? CreatedByUserId { get; set; }

        /// <summary>
        /// Người duyệt PO.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        /// <summary>
        /// Ngày duyệt.
        /// </summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// Loại tiền tệ.
        /// </summary>
        [MaxLength(10)]
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// ID dự án liên quan (liên kết PM module).
        /// </summary>
        public Guid? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
        public virtual ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
    }

    /// <summary>
    /// PurchaseOrderItem - Chi tiết đơn đặt hàng.
    /// </summary>
    public class PurchaseOrderItem : BaseEntity
    {
        /// <summary>
        /// ID đơn đặt hàng.
        /// </summary>
        public Guid PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

        /// <summary>
        /// ID sản phẩm.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Số lượng đặt.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Số lượng đã nhận.
        /// </summary>
        public int ReceivedQuantity { get; set; } = 0;

        /// <summary>
        /// Đơn giá.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Thuế suất (%).
        /// </summary>
        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 0;

        /// <summary>
        /// Giảm giá (%).
        /// </summary>
        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;

        /// <summary>
        /// Thành tiền.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal LineTotal { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
