using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaSaaS.Domain.Entities.Master
{
    public class MasterAdmin : BaseEntity
    {
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
