using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaSaaS.Domain.Entities.Master
{
    public class Payment : BaseEntity
    {
        public Guid TenantID { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public string TransectionId { get; set; } = string.Empty;
    }
}
