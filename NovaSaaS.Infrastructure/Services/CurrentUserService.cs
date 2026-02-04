using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NovaSaaS.Domain.Interfaces;

namespace NovaSaaS.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) 
                                 ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

        public Guid? TenantId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
                return Guid.TryParse(claim, out var tenantId) ? tenantId : null;
            }
        }

        public bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;
    }
}
