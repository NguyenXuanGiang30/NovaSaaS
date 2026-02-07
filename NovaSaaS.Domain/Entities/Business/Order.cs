using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.PM;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Business
{
    /// <summary>
    /// Order - Đơn hàng bán hàng.
    /// </summary>
    public class Order : BaseEntity
    {
        /// <summary>
        /// Mã đơn hàng (tự sinh).
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string OrderNumber { get; set; } = string.Empty;

        /// <summary>
        /// Ngày đặt hàng.
        /// </summary>
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID khách hàng.
        /// </summary>
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        /// <summary>
        /// Tổng tiền hàng chưa thuế.
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
        public decimal DiscountAmount { get; set; }

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
        /// Trạng thái đơn hàng.
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// Phương thức thanh toán.
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;

        /// <summary>
        /// Địa chỉ giao hàng.
        /// </summary>
        [MaxLength(500)]
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Ngày giao hàng dự kiến.
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// Ngày giao hàng thực tế.
        /// </summary>
        public DateTime? ActualDeliveryDate { get; set; }

        /// <summary>
        /// ID Warehouse xuất hàng.
        /// </summary>
        public Guid? WarehouseId { get; set; }

        /// <summary>
        /// Coupon áp dụng.
        /// </summary>
        public Guid? CouponId { get; set; }
        public virtual Coupon? Coupon { get; set; }

        /// <summary>
        /// ID nhân viên sales tạo đơn.
        /// </summary>
        public Guid? SalesUserId { get; set; }

        /// <summary>
        /// ID dự án liên quan (liên kết PM module).
        /// </summary>
        public Guid? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(2000)]
        public string? Note { get; set; }

        // Navigation
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual Invoice? Invoice { get; set; }
    }
}
