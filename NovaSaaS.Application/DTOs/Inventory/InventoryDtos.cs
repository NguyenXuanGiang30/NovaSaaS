using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Application.DTOs.Inventory
{
    #region Category DTOs

    /// <summary>
    /// DTO tạo mới Category.
    /// </summary>
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public Guid? ParentId { get; set; }
        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// DTO cập nhật Category.
    /// </summary>
    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public Guid? ParentId { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO trả về Category.
    /// </summary>
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreateAt { get; set; }
        public List<CategoryDto> Children { get; set; } = new();
    }

    #endregion

    #region Unit DTOs

    /// <summary>
    /// DTO tạo mới Unit.
    /// </summary>
    public class CreateUnitDto
    {
        [Required(ErrorMessage = "Tên đơn vị là bắt buộc")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string ShortName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cập nhật Unit.
    /// </summary>
    public class UpdateUnitDto
    {
        [Required(ErrorMessage = "Tên đơn vị là bắt buộc")]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string ShortName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO trả về Unit.
    /// </summary>
    public class UnitDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public DateTime CreateAt { get; set; }
    }

    #endregion

    #region Product DTOs

    /// <summary>
    /// DTO tạo mới Product.
    /// </summary>
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// SKU sẽ tự động tạo nếu để trống.
        /// </summary>
        [MaxLength(50)]
        public string? SKU { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá bán là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải >= 0")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá vốn phải >= 0")]
        public decimal CostPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int InitialStock { get; set; } = 0;

        public int MinStockLevel { get; set; } = 0;

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Đơn vị tính là bắt buộc")]
        public Guid UnitId { get; set; }

        /// <summary>
        /// Kho mặc định để nhập tồn kho ban đầu.
        /// </summary>
        public Guid? WarehouseId { get; set; }
    }

    /// <summary>
    /// DTO cập nhật Product.
    /// </summary>
    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá bán là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải >= 0")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá vốn phải >= 0")]
        public decimal CostPrice { get; set; }

        public int MinStockLevel { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        public Guid CategoryId { get; set; }
        public Guid UnitId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO trả về Product (danh sách).
    /// </summary>
    public class ProductListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsLowStock { get; set; }
    }

    /// <summary>
    /// DTO trả về Product (chi tiết).
    /// </summary>
    public class ProductDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int StockQuantity { get; set; }
        public int MinStockLevel { get; set; }
        public string? ImageUrl { get; set; }
        public string? Barcode { get; set; }
        public bool IsActive { get; set; }

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public List<StockByWarehouseDto> StockByWarehouse { get; set; } = new();
    }

    /// <summary>
    /// Tồn kho theo từng kho.
    /// </summary>
    public class StockByWarehouseDto
    {
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string? Location { get; set; }
    }

    #endregion

    #region Warehouse DTOs

    /// <summary>
    /// DTO tạo mới Warehouse.
    /// </summary>
    public class CreateWarehouseDto
    {
        [Required(ErrorMessage = "Tên kho là bắt buộc")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }
    }

    /// <summary>
    /// DTO cập nhật Warehouse.
    /// </summary>
    public class UpdateWarehouseDto
    {
        [Required(ErrorMessage = "Tên kho là bắt buộc")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }
    }

    /// <summary>
    /// DTO trả về Warehouse.
    /// </summary>
    public class WarehouseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public DateTime CreateAt { get; set; }
    }

    #endregion

    #region Stock Movement DTOs

    /// <summary>
    /// DTO nhập/xuất kho nhanh (simple adjustment).
    /// </summary>
    public class QuickStockAdjustmentDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid WarehouseId { get; set; }

        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// True = Nhập kho, False = Xuất kho.
        /// </summary>
        public bool IsInbound { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? ReferenceCode { get; set; }
    }

    /// <summary>
    /// DTO chuyển kho nhanh (simple transfer).
    /// </summary>
    public class QuickStockTransferDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid FromWarehouseId { get; set; }

        [Required]
        public Guid ToWarehouseId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải > 0")]
        public int Quantity { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO trả về lịch sử biến động kho.
    /// </summary>
    public class StockMovementDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Type { get; set; } = string.Empty;
        public int QuantityBefore { get; set; }
        public int QuantityAfter { get; set; }
        public string? ReferenceCode { get; set; }
        public string? Notes { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreateAt { get; set; }
    }

    #endregion
}
