using System;

namespace NovaSaaS.Domain.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        Guid? TenantId { get; }
        bool IsAdmin { get; }
    }
}
