using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.Identity;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NovaSaaS.Infrastructure.Persistence
{
    /// <summary>
    /// DatabaseInitializer - "Kỹ sư trưởng" phụ trách việc tự động hóa hạ tầng cơ sở dữ liệu.
    /// 
    /// 3 Chức năng chính:
    /// 1. Khởi tạo Schema (CREATE SCHEMA)
    /// 2. Triển khai cấu trúc bảng (EF Core Migrations)
    /// 3. Đổ dữ liệu mồi (Roles, Permissions, Admin User, Settings)
    /// </summary>
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Khởi tạo toàn bộ hạ tầng database cho một Tenant mới.
        /// </summary>
        public async Task InitializeTenantAsync(string schemaName, Guid tenantId, string adminEmail, string adminPassword)
        {
            // Bước 0: Sanitize schema name để tránh SQL Injection
            var sanitizedSchema = SanitizeSchemaName(schemaName);
            _logger.LogInformation("🏗️ Bắt đầu khởi tạo hạ tầng cho Tenant Schema: {Schema}", sanitizedSchema);

            using var scope = _serviceProvider.CreateScope();

            // QUAN TRỌNG: Set tenant TRƯỚC khi resolve DbContext
            // Điều này đảm bảo EF Core sẽ build model với đúng schema
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
            tenantService.SetTenant(tenantId, sanitizedSchema);

            // Bây giờ mới resolve DbContext (với schema đã được set)
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // ========================================
                // BƯỚC 1: Tạo Schema mới (San lấp mặt bằng)
                // ========================================
                _logger.LogInformation("📦 Bước 1/3: Tạo Schema '{Schema}'...", sanitizedSchema);
                await CreateSchemaAsync(dbContext, sanitizedSchema);

                // ========================================
                // BƯỚC 2: Chạy Migrations (Xây khung 27 bảng)
                // ========================================
                _logger.LogInformation("🔧 Bước 2/3: Chạy EF Core Migrations...");
                await RunMigrationsAsync(dbContext);

                // ========================================
                // BƯỚC 3: Seed dữ liệu mồi (Nội thất cơ bản)
                // ========================================
                _logger.LogInformation("🌱 Bước 3/3: Seed dữ liệu mồi...");
                await SeedInitialDataAsync(dbContext, adminEmail, adminPassword);

                _logger.LogInformation("✅ Hoàn tất khởi tạo hạ tầng cho Schema: {Schema}", sanitizedSchema);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi khởi tạo hạ tầng cho Schema: {Schema}", sanitizedSchema);
                
                // Thử cleanup schema nếu có lỗi
                await TryCleanupSchemaAsync(dbContext, sanitizedSchema);
                
                throw; // Re-throw để TenantRegistrationService có thể rollback
            }
        }

        #region Bước 1: Tạo Schema

        /// <summary>
        /// Tạo PostgreSQL Schema mới cho Tenant.
        /// </summary>
        private async Task CreateSchemaAsync(ApplicationDbContext dbContext, string schemaName)
        {
            // Sử dụng ExecuteSqlRaw với tham số đã được sanitize
            // Lưu ý: Schema name không thể parameterized trong SQL, nên phải sanitize trước
            var sql = $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"";
            await dbContext.Database.ExecuteSqlRawAsync(sql);
            
            _logger.LogDebug("Schema '{Schema}' đã được tạo hoặc đã tồn tại", schemaName);
        }

        #endregion

        #region Bước 2: Chạy Migrations

        /// <summary>
        /// Áp dụng tất cả EF Core Migrations vào schema hiện tại.
        /// Đây là cách IMigrator tự động tạo 27 bảng với đúng schema context.
        /// </summary>
        private async Task RunMigrationsAsync(ApplicationDbContext dbContext)
        {
            var migrator = dbContext.Database.GetService<IMigrator>();
            
            // MigrateAsync() sẽ tự động detect schema từ DbContext
            // và áp dụng tất cả pending migrations
            await migrator.MigrateAsync();
            
            _logger.LogDebug("Migrations đã được áp dụng thành công");
        }

        #endregion

        #region Bước 3: Seed dữ liệu mồi

        /// <summary>
        /// Seed toàn bộ dữ liệu cần thiết cho Tenant mới.
        /// </summary>
        private async Task SeedInitialDataAsync(ApplicationDbContext dbContext, string adminEmail, string adminPassword)
        {
            // 3.1: Seed Permissions (Quyền hạn cơ bản)
            var permissions = await SeedPermissionsAsync(dbContext);

            // 3.2: Seed Roles (4 vai trò mặc định)
            var roles = await SeedRolesAsync(dbContext, permissions);

            // 3.3: Seed Admin User
            var adminUser = await SeedAdminUserAsync(dbContext, adminEmail, adminPassword);

            // 3.4: Gán Admin Role cho Admin User
            await AssignRoleToUserAsync(dbContext, adminUser.Id, roles["Admin"].Id);

            // 3.5: Seed Tenant Settings (Cài đặt mặc định)
            await SeedTenantSettingsAsync(dbContext);

            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Seed các Permission cơ bản cho hệ thống.
        /// </summary>
        private async Task<Dictionary<string, Permission>> SeedPermissionsAsync(ApplicationDbContext dbContext)
        {
            var permissions = new Dictionary<string, Permission>();

            var defaultPermissions = new List<(string Code, string Description)>
            {
                // Products
                ("products.view", "Xem danh sách sản phẩm"),
                ("products.create", "Tạo sản phẩm mới"),
                ("products.edit", "Chỉnh sửa sản phẩm"),
                ("products.delete", "Xóa sản phẩm"),
                
                // Orders
                ("orders.view", "Xem danh sách đơn hàng"),
                ("orders.create", "Tạo đơn hàng mới"),
                ("orders.edit", "Chỉnh sửa đơn hàng"),
                ("orders.delete", "Xóa đơn hàng"),
                
                // Customers
                ("customers.view", "Xem danh sách khách hàng"),
                ("customers.create", "Tạo khách hàng mới"),
                ("customers.edit", "Chỉnh sửa khách hàng"),
                ("customers.delete", "Xóa khách hàng"),
                
                // Inventory
                ("inventory.view", "Xem kho hàng"),
                ("inventory.manage", "Quản lý xuất nhập kho"),
                
                // Reports
                ("reports.view", "Xem báo cáo"),
                ("reports.export", "Xuất báo cáo"),
                
                // Users (Admin only)
                ("users.view", "Xem danh sách người dùng"),
                ("users.create", "Tạo người dùng mới"),
                ("users.edit", "Chỉnh sửa người dùng"),
                ("users.delete", "Xóa người dùng"),
                
                // Settings (Admin only)
                ("settings.view", "Xem cài đặt"),
                ("settings.edit", "Chỉnh sửa cài đặt")
            };

            foreach (var (code, description) in defaultPermissions)
            {
                if (!await dbContext.Permissions.AnyAsync(p => p.Code == code))
                {
                    var permission = new Permission
                    {
                        Id = Guid.NewGuid(),
                        Code = code,
                        Description = description,
                        CreateAt = DateTime.UtcNow
                    };
                    dbContext.Permissions.Add(permission);
                    permissions[code] = permission;
                }
            }

            await dbContext.SaveChangesAsync();
            _logger.LogDebug("Đã seed {Count} permissions", permissions.Count);
            
            return permissions;
        }

        /// <summary>
        /// Seed 4 vai trò mặc định: Admin, Manager, Staff, User.
        /// </summary>
        private async Task<Dictionary<string, Role>> SeedRolesAsync(ApplicationDbContext dbContext, Dictionary<string, Permission> permissions)
        {
            var roles = new Dictionary<string, Role>();

            var roleDefinitions = new List<(string Name, string[] PermissionCodes)>
            {
                ("Admin", new[] 
                { 
                    "products.view", "products.create", "products.edit", "products.delete",
                    "orders.view", "orders.create", "orders.edit", "orders.delete",
                    "customers.view", "customers.create", "customers.edit", "customers.delete",
                    "inventory.view", "inventory.manage",
                    "reports.view", "reports.export",
                    "users.view", "users.create", "users.edit", "users.delete",
                    "settings.view", "settings.edit"
                }),
                ("Manager", new[] 
                { 
                    "products.view", "products.create", "products.edit",
                    "orders.view", "orders.create", "orders.edit",
                    "customers.view", "customers.create", "customers.edit",
                    "inventory.view", "inventory.manage",
                    "reports.view", "reports.export",
                    "users.view"
                }),
                ("Staff", new[] 
                { 
                    "products.view",
                    "orders.view", "orders.create",
                    "customers.view", "customers.create",
                    "inventory.view"
                }),
                ("User", new[] 
                { 
                    "products.view",
                    "orders.view"
                })
            };

            foreach (var (name, permissionCodes) in roleDefinitions)
            {
                var existingRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == name);
                
                if (existingRole == null)
                {
                    var role = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        CreateAt = DateTime.UtcNow
                    };
                    dbContext.Roles.Add(role);
                    roles[name] = role;

                    // Gán permissions cho role
                    foreach (var permCode in permissionCodes)
                    {
                        if (permissions.TryGetValue(permCode, out var permission))
                        {
                            dbContext.RolePermissions.Add(new RolePermission
                            {
                                RoleId = role.Id,
                                PermissionId = permission.Id
                            });
                        }
                    }
                }
                else
                {
                    roles[name] = existingRole;
                }
            }

            await dbContext.SaveChangesAsync();
            _logger.LogDebug("Đã seed {Count} roles với permissions", roles.Count);
            
            return roles;
        }

        /// <summary>
        /// Tạo tài khoản Admin đầu tiên cho Tenant.
        /// </summary>
        private async Task<User> SeedAdminUserAsync(ApplicationDbContext dbContext, string adminEmail, string adminPassword)
        {
            var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            
            if (existingUser != null)
            {
                _logger.LogDebug("Admin user đã tồn tại: {Email}", adminEmail);
                return existingUser;
            }

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                FullName = "Tenant Admin",
                IsActive = true,
                CreateAt = DateTime.UtcNow
            };

            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();
            
            _logger.LogDebug("Đã tạo Admin user: {Email}", adminEmail);
            return adminUser;
        }

        /// <summary>
        /// Gán Role cho User thông qua bảng UserRole.
        /// </summary>
        private async Task AssignRoleToUserAsync(ApplicationDbContext dbContext, Guid userId, Guid roleId)
        {
            var exists = await dbContext.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            
            if (!exists)
            {
                dbContext.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });
                await dbContext.SaveChangesAsync();
                _logger.LogDebug("Đã gán Role {RoleId} cho User {UserId}", roleId, userId);
            }
        }

        /// <summary>
        /// Seed cài đặt mặc định cho Tenant (ngôn ngữ, màu sắc...).
        /// </summary>
        private async Task SeedTenantSettingsAsync(ApplicationDbContext dbContext)
        {
            if (!await dbContext.Set<TenantSetting>().AnyAsync())
            {
                var settings = new TenantSetting
                {
                    Id = Guid.NewGuid(),
                    PrimaryColor = "#3b82f6",  // Blue
                    Language = "vi",            // Tiếng Việt
                    CompanyName = "",           // Sẽ được cập nhật bởi Tenant
                    LogoUrl = "",
                    CreateAt = DateTime.UtcNow
                };

                dbContext.Set<TenantSetting>().Add(settings);
                await dbContext.SaveChangesAsync();
                _logger.LogDebug("Đã seed TenantSettings mặc định");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Sanitize schema name để tránh SQL Injection.
        /// Chỉ cho phép: chữ cái, số và underscore.
        /// </summary>
        private static string SanitizeSchemaName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Schema name không được để trống", nameof(name));

            // Chỉ giữ lại alphanumeric và underscore
            var sanitized = Regex.Replace(name, @"[^a-zA-Z0-9_]", "");
            
            // Đảm bảo không bắt đầu bằng số
            if (char.IsDigit(sanitized[0]))
            {
                sanitized = "tenant_" + sanitized;
            }

            return sanitized.ToLowerInvariant();
        }

        /// <summary>
        /// Thử xóa schema nếu khởi tạo thất bại (cleanup).
        /// </summary>
        private async Task TryCleanupSchemaAsync(ApplicationDbContext dbContext, string schemaName)
        {
            try
            {
                _logger.LogWarning("🧹 Đang cleanup schema '{Schema}' do lỗi...", schemaName);
                
                // CASCADE sẽ xóa tất cả objects trong schema
                var sql = $"DROP SCHEMA IF EXISTS \"{schemaName}\" CASCADE";
                await dbContext.Database.ExecuteSqlRawAsync(sql);
                
                _logger.LogInformation("Schema '{Schema}' đã được cleanup", schemaName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể cleanup schema '{Schema}'", schemaName);
                // Không throw - đây chỉ là best effort cleanup
            }
        }

        #endregion
    }
}
