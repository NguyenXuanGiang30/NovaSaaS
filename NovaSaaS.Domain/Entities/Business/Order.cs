using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NovaSaaS.Domain.Entities.Business
{
    public class Order : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string OrderNumber { get; set; } = string.Empty; // Fixed typo
        
        public Guid CustomerId { get; set; }
        public virtual Customer Customer { get; set; } = null!;
        
        [Range(0, double.MaxValue)]
        public decimal SubTotal { get; set; } // Tổng tiền hàng chưa thuế
        
        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; } // Thuế
        
        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } // Giảm giá

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; } // Tổng thanh toán
        
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        public Guid? CouponId { get; set; }
        public virtual Coupon? Coupon { get; set; }
        
        public string? Note { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual Invoice? Invoice { get; set; }
    }
}
