using Microsoft.AspNetCore.Http;
using NovaSaaS.Domain.Interfaces;

namespace NovaSaaS.Infrastructure.Services
{
    public class TenantService : ITenantService
    {
        public string? SchemaName { get; private set; } = "public";
        public Guid? TenantId { get; private set; }

        public void SetTenant(Guid tenantId, string schemaName)
        {
            TenantId = tenantId;
            SchemaName = schemaName;
        }
    }
}
