using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NovaSaaS.Domain.Entities.Master
{
    public class PlanFeature : BaseEntity
    {
        public Guid PlanId { get; set; }
        public virtual SubscriptionPlan Plan { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string FeatureCode { get; set; } = string.Empty; // e.g. AI_CHAT
        
        public bool IsEnabled { get; set; }
    }
}
