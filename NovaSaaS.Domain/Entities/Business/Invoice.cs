using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NovaSaaS.Domain.Enums;

namespace NovaSaaS.Domain.Entities.Business
{
    public class Invoice : BaseEntity
    {
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        
        [Required]
        [MaxLength(20)]
        public string InvoiceNumber { get; set; } = string.Empty;
        
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }
        
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        
        public DateTime? DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
    }
}
