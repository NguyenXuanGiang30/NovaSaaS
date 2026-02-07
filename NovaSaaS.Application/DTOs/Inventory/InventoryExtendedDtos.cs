using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Application.DTOs.Inventory
{
    // ==================== ProductVariant DTOs ====================

    public class CreateProductVariantDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [MaxLength(50)]
        public string? SKU { get; set; }

        [Required(ErrorMessage = "Tên biến thể là bắt buộc")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? AttributeName1 { get; set; }

        [MaxLength(100)]
        public string? AttributeValue1 { get; set; }

        [MaxLength(100)]
        public string? AttributeName2 { get; set; }

        [MaxLength(100)]
        public string? AttributeValue2 { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? CostPrice { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }
    }

    public class UpdateProductVariantDto
    {
        [Required(ErrorMessage = "Tên biến thể là bắt buộc")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? AttributeName1 { get; set; }

        [MaxLength(100)]
        public string? AttributeValue1 { get; set; }

        [MaxLength(100)]
        public string? AttributeName2 { get; set; }

        [MaxLength(100)]
        public string? AttributeValue2 { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? CostPrice { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ProductVariantDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? AttributeName1 { get; set; }
        public string? AttributeValue1 { get; set; }
        public string? AttributeName2 { get; set; }
        public string? AttributeValue2 { get; set; }
        public decimal? Price { get; set; }
        public decimal? CostPrice { get; set; }
        public string? Barcode { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreateAt { get; set; }
    }

    // ==================== Location DTOs ====================

    public class CreateLocationDto
    {
        [Required]
        public Guid WarehouseId { get; set; }

        [Required(ErrorMessage = "Mã vị trí là bắt buộc")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(20)]
        public string? Aisle { get; set; }

        [MaxLength(20)]
        public string? Rack { get; set; }

        [MaxLength(20)]
        public string? Shelf { get; set; }

        public int? MaxCapacity { get; set; }
    }

    public class UpdateLocationDto
    {
        [Required(ErrorMessage = "Mã vị trí là bắt buộc")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(20)]
        public string? Aisle { get; set; }

        [MaxLength(20)]
        public string? Rack { get; set; }

        [MaxLength(20)]
        public string? Shelf { get; set; }

        public int? MaxCapacity { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class LocationDto
    {
        public Guid Id { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Aisle { get; set; }
        public string? Rack { get; set; }
        public string? Shelf { get; set; }
        public bool IsActive { get; set; }
        public int? MaxCapacity { get; set; }
        public DateTime CreateAt { get; set; }
    }

    // ==================== StockAdjustment DTOs ====================

    public class CreateStockAdjustmentDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }

        public StockAdjustmentType Type { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải > 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Lý do điều chỉnh là bắt buộc")]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class StockAdjustmentDetailDto
    {
        public Guid Id { get; set; }
        public string AdjustmentNumber { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public StockAdjustmentType Type { get; set; }
        public string TypeName => Type.ToString();
        public int Quantity { get; set; }
        public int QuantityBefore { get; set; }
        public int QuantityAfter { get; set; }
        public string Reason { get; set; } = string.Empty;
        public StockAdjustmentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Notes { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreateAt { get; set; }
    }

    public class StockAdjustmentFilterDto
    {
        public Guid? ProductId { get; set; }
        public Guid? WarehouseId { get; set; }
        public StockAdjustmentType? Type { get; set; }
        public StockAdjustmentStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // ==================== StockTransfer DTOs ====================

    public class CreateStockTransferDto
    {
        [Required]
        public Guid FromWarehouseId { get; set; }

        [Required]
        public Guid ToWarehouseId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public List<CreateStockTransferItemDto> Items { get; set; } = new();
    }

    public class CreateStockTransferItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải > 0")]
        public int Quantity { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class StockTransferDto
    {
        public Guid Id { get; set; }
        public string TransferNumber { get; set; } = string.Empty;
        public Guid FromWarehouseId { get; set; }
        public string FromWarehouseName { get; set; } = string.Empty;
        public Guid ToWarehouseId { get; set; }
        public string ToWarehouseName { get; set; } = string.Empty;
        public StockTransferStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? Notes { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreateAt { get; set; }
        public int TotalItems { get; set; }
        public int TotalQuantity { get; set; }
        public List<StockTransferItemDto> Items { get; set; } = new();
    }

    public class StockTransferItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public string? Notes { get; set; }
    }

    public class StockTransferFilterDto
    {
        public Guid? FromWarehouseId { get; set; }
        public Guid? ToWarehouseId { get; set; }
        public StockTransferStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UpdateStockTransferStatusDto
    {
        public StockTransferStatus NewStatus { get; set; }
        public string? Notes { get; set; }
    }

    // ==================== InventoryCount DTOs ====================

    public class CreateInventoryCountDto
    {
        [Required]
        public Guid WarehouseId { get; set; }

        public DateTime? CountDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public List<CreateInventoryCountItemDto> Items { get; set; } = new();
    }

    public class CreateInventoryCountItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Range(0, int.MaxValue)]
        public int ActualQuantity { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class InventoryCountDto
    {
        public Guid Id { get; set; }
        public string CountNumber { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public InventoryCountStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CountDate { get; set; }
        public string? Notes { get; set; }
        public int TotalDiscrepancy { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreateAt { get; set; }
        public int TotalItems { get; set; }
        public List<InventoryCountItemDto> Items { get; set; } = new();
    }

    public class InventoryCountItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int SystemQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public int Discrepancy => ActualQuantity - SystemQuantity;
        public string? Notes { get; set; }
    }

    public class InventoryCountFilterDto
    {
        public Guid? WarehouseId { get; set; }
        public InventoryCountStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // ==================== LotNumber DTOs ====================

    public class CreateLotNumberDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }

        [Required(ErrorMessage = "Mã lô là bắt buộc")]
        [MaxLength(50)]
        public string LotCode { get; set; } = string.Empty;

        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [MaxLength(200)]
        public string? SupplierName { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class LotNumberDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string LotCode { get; set; } = string.Empty;
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public int SoldQuantity { get; set; }
        public int RemainingQuantity => Quantity - SoldQuantity;
        public LotStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? SupplierName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateAt { get; set; }
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    }

    public class LotNumberFilterDto
    {
        public Guid? ProductId { get; set; }
        public Guid? WarehouseId { get; set; }
        public LotStatus? Status { get; set; }
        public bool? IsExpired { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // ==================== SerialNumber DTOs ====================

    public class CreateSerialNumberDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }

        [Required(ErrorMessage = "Mã serial là bắt buộc")]
        [MaxLength(100)]
        public string Serial { get; set; } = string.Empty;

        public Guid? LotNumberId { get; set; }
        public DateTime? WarrantyExpiry { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class CreateSerialNumberBatchDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }

        public Guid? LotNumberId { get; set; }
        public DateTime? WarrantyExpiry { get; set; }

        [Required]
        public List<string> Serials { get; set; } = new();
    }

    public class SerialNumberDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public string Serial { get; set; } = string.Empty;
        public SerialNumberStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public Guid? LotNumberId { get; set; }
        public string? LotCode { get; set; }
        public Guid? SoldOrderId { get; set; }
        public DateTime? SoldDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateAt { get; set; }
    }

    public class SerialNumberFilterDto
    {
        public Guid? ProductId { get; set; }
        public Guid? WarehouseId { get; set; }
        public SerialNumberStatus? Status { get; set; }
        public Guid? LotNumberId { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
