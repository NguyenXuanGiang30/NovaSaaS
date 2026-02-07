using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Inventory
{
    /// <summary>
    /// Warehouse - Kho hàng trong hệ thống quản lý tồn kho.
    /// </summary>
    public class Warehouse : BaseEntity
    {
        /// <summary>
        /// Mã kho hàng.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Tên kho hàng.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ kho.
        /// </summary>
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại liên hệ.
        /// </summary>
        [MaxLength(20)]
        [Phone]
        public string? Phone { get; set; }

        /// <summary>
        /// Email liên hệ.
        /// </summary>
        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// ID người quản lý kho.
        /// </summary>
        public Guid? ManagerUserId { get; set; }

        /// <summary>
        /// Kho mặc định hay không.
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation
        public virtual ICollection<Stock> Stocks { get; set; } = new List<Stock>();
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
        public virtual ICollection<LotNumber> LotNumbers { get; set; } = new List<LotNumber>();
        public virtual ICollection<SerialNumber> SerialNumbers { get; set; } = new List<SerialNumber>();
        public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();
        public virtual ICollection<InventoryCount> InventoryCounts { get; set; } = new List<InventoryCount>();
    }
}
