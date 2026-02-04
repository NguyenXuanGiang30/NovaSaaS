using System;
using System.Collections.Generic;
using System.Text;

namespace NovaSaaS.Domain.Entities.Common
{
    public class AuditLog : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Create, Update, Delete
        public string EntityName {  get; set; } = string.Empty;
        public string EntityId {  get; set; } = string.Empty;
        public string? OldValues { get; set; } // JSON format
        public string? NewValues { get; set; } // JSON format
    }
}
