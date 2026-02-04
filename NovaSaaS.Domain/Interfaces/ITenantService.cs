using System;

namespace NovaSaaS.Domain.Interfaces
{
    public interface ITenantService
    {
        string? SchemaName { get; }
        Guid? TenantId { get; }
        void SetTenant(Guid tenantId, string schemaName);
    }
}
