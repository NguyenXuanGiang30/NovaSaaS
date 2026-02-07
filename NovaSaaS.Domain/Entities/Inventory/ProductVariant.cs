using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// ProductVariant - Biến thể sản phẩm (kích thước, màu sắc, v.v.).
    /// </summary>
    public class ProductVariant : BaseEntity
    {
        /// <summary>
        /// ID sản phẩm gốc.
        /// </summary>
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        /// <summary>
        /// Mã SKU riêng cho biến thể.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Tên biến thể (ví dụ: "Đỏ - XL").
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Thuộc tính 1 (ví dụ: Màu sắc).
        /// </summary>
        [MaxLength(100)]
        public string? AttributeName1 { get; set; }

        /// <summary>
        /// Giá trị thuộc tính 1 (ví dụ: Đỏ).
        /// </summary>
        [MaxLength(100)]
        public string? AttributeValue1 { get; set; }

        /// <summary>
        /// Thuộc tính 2 (ví dụ: Kích thước).
        /// </summary>
        [MaxLength(100)]
        public string? AttributeName2 { get; set; }

        /// <summary>
        /// Giá trị thuộc tính 2 (ví dụ: XL).
        /// </summary>
        [MaxLength(100)]
        public string? AttributeValue2 { get; set; }

        /// <summary>
        /// Giá bán biến thể (nếu khác giá gốc, null = dùng giá gốc).
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        /// <summary>
        /// Giá vốn biến thể.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? CostPrice { get; set; }

        /// <summary>
        /// Mã vạch riêng.
        /// </summary>
        [MaxLength(100)]
        public string? Barcode { get; set; }

        /// <summary>
        /// Đường dẫn ảnh.
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Tồn kho tổng (cache).
        /// </summary>
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; } = 0;
    }
}
