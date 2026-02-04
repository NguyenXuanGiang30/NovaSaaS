using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaSaaS.Domain.Entities.Master
{
    public class GlobalAuditLog : BaseEntity
    {
        public string Event { get; set;} = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }
}
