using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NovaSaaS.Domain.Entities.Business
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}
