using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace NovaSaaS.Domain.Entities.Identity
{
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;  
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
