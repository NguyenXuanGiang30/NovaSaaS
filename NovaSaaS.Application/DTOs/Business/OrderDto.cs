using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Application.DTOs.Business
{
    public class CreateOrderDto
    {
        [Required]
        public Guid CustomerId { get; set; }
        
        public Guid? CouponId { get; set; }
        
        public string? Note { get; set; }

        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        [Required]
        public Guid ProductId { get; set; }
        
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Required]
        public Guid WarehouseId { get; set; } // Cần chọn kho xuất
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreateAt { get; set; }
        public string? Note { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public InvoiceDto? Invoice { get; set; }
    }

    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public class OrderFilterDto
    {
        public Guid? CustomerId { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UpdateOrderStatusDto
    {
        public OrderStatus NewStatus { get; set; }
        public string? Note { get; set; }
    }

    public class CancelOrderRequest
    {
        public string Reason { get; set; } = string.Empty;
        public bool RestoreStock { get; set; } = true;
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}
