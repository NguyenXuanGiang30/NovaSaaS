using NovaSaaS.Domain.Entities.Accounting;
using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// Sản phẩm - Entity chính của hệ thống kho hàng.
    /// </summary>
    public class Product : BaseEntity
    {
        /// <summary>
        /// Tên sản phẩm.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mã SKU - Duy nhất trong phạm vi Tenant.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả sản phẩm (dùng cho AI Knowledge Base).
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// Giá bán.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        /// <summary>
        /// Giá vốn/Giá nhập.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }

        /// <summary>
        /// Số lượng tồn kho tổng (cache từ bảng Stock).
        /// </summary>
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        /// <summary>
        /// Mức tồn kho tối thiểu để cảnh báo.
        /// </summary>
        public int MinStockLevel { get; set; } = 0;

        /// <summary>
        /// Đường dẫn ảnh sản phẩm.
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Mã vạch (Barcode).
        /// </summary>
        [MaxLength(100)]
        public string? Barcode { get; set; }

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Foreign Keys
        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        public Guid UnitId { get; set; }
        public virtual Unit Unit { get; set; } = null!;

        /// <summary>
        /// TK hàng tồn kho (VD: 156) - liên kết ACC module.
        /// </summary>
        public Guid? InventoryAccountId { get; set; }
        public virtual ChartOfAccount? InventoryAccount { get; set; }

        /// <summary>
        /// TK doanh thu bán hàng (VD: 511) - liên kết ACC module.
        /// </summary>
        public Guid? RevenueAccountId { get; set; }
        public virtual ChartOfAccount? RevenueAccount { get; set; }

        /// <summary>
        /// TK giá vốn hàng bán (VD: 632) - liên kết ACC module.
        /// </summary>
        public Guid? CogsAccountId { get; set; }
        public virtual ChartOfAccount? CogsAccount { get; set; }

        // Navigation
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<LotNumber> LotNumbers { get; set; } = new List<LotNumber>();
        public virtual ICollection<SerialNumber> SerialNumbers { get; set; } = new List<SerialNumber>();
    }
}
