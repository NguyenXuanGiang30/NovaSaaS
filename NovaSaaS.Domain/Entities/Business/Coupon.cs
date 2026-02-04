using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NovaSaaS.Domain.Entities.Business
{
    public class Coupon : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal DiscountValue { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
