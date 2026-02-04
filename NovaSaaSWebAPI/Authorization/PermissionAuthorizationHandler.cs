using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Domain.Interfaces;
using System.Security.Claims;

namespace NovaSaaS.WebApi.Authorization
{
    /// <summary>
    /// Attribute để yêu cầu permission cụ thể cho action.
    /// Ví dụ: [RequirePermission("inventory.view")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public string Permission { get; }
        public bool RequireAll { get; set; } = false;

        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public RequirePermissionAttribute(params string[] permissions)
        {
            Permission = string.Join(",", permissions);
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Auth được xử lý bởi PermissionAuthorizationHandler
        }
    }

    /// <summary>
    /// Permission requirement cho policy-based authorization.
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }
        public bool RequireAll { get; }

        public PermissionRequirement(string permission, bool requireAll = false)
        {
            Permission = permission;
            RequireAll = requireAll;
        }
    }

    /// <summary>
    /// Handler xử lý permission authorization.
    /// Kiểm tra user có permission cần thiết không.
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(
            IServiceProvider serviceProvider,
            ILogger<PermissionAuthorizationHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return;
            }

            // Sử dụng scoped service
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var cacheService = scope.ServiceProvider.GetService<NovaSaaS.Application.Interfaces.Caching.ICacheService>();

            // Check cache first
            var cacheKey = $"user:{parsedUserId}:permissions";
            var cachedPermissions = await cacheService?.GetAsync<HashSet<string>>(cacheKey)!;

            HashSet<string> userPermissions;
            if (cachedPermissions != null)
            {
                userPermissions = cachedPermissions;
            }
            else
            {
                // Load from database
                userPermissions = await LoadUserPermissionsAsync(unitOfWork, parsedUserId);
                
                // Cache for 10 minutes
                if (cacheService != null)
                {
                    await cacheService.SetAsync(cacheKey, userPermissions, TimeSpan.FromMinutes(10));
                }
            }

            // Check permission(s)
            var requiredPermissions = requirement.Permission.Split(',', StringSplitOptions.RemoveEmptyEntries);

            bool hasPermission;
            if (requirement.RequireAll)
            {
                hasPermission = requiredPermissions.All(p => userPermissions.Contains(p.Trim()));
            }
            else
            {
                hasPermission = requiredPermissions.Any(p => userPermissions.Contains(p.Trim()));
            }

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User {UserId} denied access - missing permission: {Permission}",
                    parsedUserId, requirement.Permission);
            }
        }

        private async Task<HashSet<string>> LoadUserPermissionsAsync(IUnitOfWork unitOfWork, Guid userId)
        {
            // Get user with roles and permissions using expression includes
            var user = await unitOfWork.Users.GetByIdAsync(userId, u => u.UserRoles);

            if (user == null || user.UserRoles == null || !user.UserRoles.Any())
            {
                return new HashSet<string>();
            }

            // Get role IDs
            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
            
            // Get roles with permissions
            var roles = await unitOfWork.Roles.FindAsync(r => roleIds.Contains(r.Id), r => r.RolePermissions);
            
            // Get permission IDs
            var permissionIds = roles.SelectMany(r => r.RolePermissions)
                .Select(rp => rp.PermissionId)
                .Distinct()
                .ToList();
            
            // Get permissions
            var permissions = await unitOfWork.Permissions.FindAsync(p => permissionIds.Contains(p.Id));

            return permissions.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Extension methods để cấu hình permission-based authorization.
    /// </summary>
    public static class PermissionAuthorizationExtensions
    {
        /// <summary>
        /// Thêm permission authorization vào services.
        /// </summary>
        public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.AddAuthorization(options =>
            {
                // Tạo policies cho các permissions phổ biến
                var commonPermissions = new[]
                {
                    // Inventory
                    "inventory.view", "inventory.create", "inventory.edit", "inventory.delete",
                    "products.view", "products.create", "products.edit", "products.delete",
                    "categories.view", "categories.manage",
                    "stock.view", "stock.adjust", "stock.transfer",
                    
                    // Orders
                    "orders.view", "orders.create", "orders.edit", "orders.cancel",
                    "invoices.view", "invoices.create", "invoices.edit",
                    
                    // Customers
                    "customers.view", "customers.create", "customers.edit", "customers.delete",
                    
                    // AI
                    "ai.chat", "ai.documents.upload", "ai.documents.manage",
                    
                    // Admin
                    "users.view", "users.invite", "users.manage",
                    "roles.view", "roles.manage",
                    "settings.view", "settings.manage",
                    "reports.view", "reports.export",
                    "audit.view"
                };

                foreach (var permission in commonPermissions)
                {
                    options.AddPolicy($"Permission:{permission}",
                        policy => policy.Requirements.Add(new PermissionRequirement(permission)));
                }

                // Admin policy - requires any admin permission
                options.AddPolicy("AdminAccess",
                    policy => policy.Requirements.Add(
                        new PermissionRequirement("users.manage,roles.manage,settings.manage")));
            });

            return services;
        }
    }
}
