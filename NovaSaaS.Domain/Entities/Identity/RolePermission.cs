using System;
using System.Collections.Generic;
using System.Text;

namespace NovaSaaS.Domain.Entities.Identity
{
    public class RolePermission
    {
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
        public Guid PermissionId { get; set; }
        public virtual Permission Permission { get; set; } = null!;
    }
}
