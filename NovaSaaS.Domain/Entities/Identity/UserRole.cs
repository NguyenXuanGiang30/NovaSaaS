using System;
using System.Collections.Generic;
using System.Text;

namespace NovaSaaS.Domain.Entities.Identity
{
    public class UserRole
    {   
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
    }
}
