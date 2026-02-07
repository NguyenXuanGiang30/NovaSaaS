using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NovaSaaS.Domain.Entities.Business
{
    public class Customer : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(20)]
        public string TaxCode { get; set; } = string.Empty;

        public CustomerRank Rank { get; set; } = CustomerRank.Standard;
        
        public CustomerType Type { get; set; } = CustomerType.Retail;

        [Range(0, double.MaxValue)]
        public decimal TotalSpending { get; set; } = 0;
        
        public string? Address { get; set; }
        public string? Email { get; set; }

        /// <summary>
        /// Nhóm khách hàng (tùy chọn).
        /// </summary>
        public Guid? CustomerGroupId { get; set; }
        public virtual CustomerGroup? CustomerGroup { get; set; }

        /// <summary>
        /// Tổng điểm loyalty hiện tại.
        /// </summary>
        public int LoyaltyPoints { get; set; } = 0;

        // Navigation
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
    }
}
