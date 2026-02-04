using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NovaSaaS.Domain.Entities.Inventory
{
    public class Warehouse : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public string Adress { get; set; } = string.Empty;
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    }
}
