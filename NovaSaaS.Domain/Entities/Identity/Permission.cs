using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaSaaS.Domain.Entities.Identity
{
    public class Permission : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
