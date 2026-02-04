using System;
using System.Collections.Generic;
using System.Text;

namespace NovaSaaS.Domain.Entities.Common
{
    public class TenantSetting : BaseEntity
    {
        public string PrimaryColor { get; set; } = "#3b82f6";
        public string LogoUrl { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Language { get; set; } = "vi";
    }
}
