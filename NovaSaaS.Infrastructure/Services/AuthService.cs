using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NovaSaaS.Application.DTOs;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Domain.Entities.Identity;
using NovaSaaS.Domain.Entities.Master;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.Infrastructure.Configurations;
using NovaSaaS.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NovaSaaS.Infrastructure.Services
{
    /// <summary>
    /// AuthService - Xử lý toàn bộ logic xác thực và quản lý JWT.
    /// 
    /// Quy trình:
    /// 1. Tìm Tenant từ Subdomain (schema public)
    /// 2. Chuyển sang Schema của Tenant
    /// 3. Xác thực password với BCrypt
    /// 4. Tạo JWT với SaaS Claims (tenant_id, schema_name, permissions)
    /// 5. Lưu Refresh Token vào DB
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IServiceProvider serviceProvider,
            IRepository<Tenant> tenantRepository,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _serviceProvider = serviceProvider;
            _tenantRepository = tenantRepository;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        #region Login

        /// <summary>
        /// Đăng nhập với email, password và subdomain.
        /// </summary>
        public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress = null)
        {
            _logger.LogInformation("🔐 Đăng nhập: {Email} @ {Subdomain}", request.Email, request.Subdomain);

            // 1. Tìm Tenant từ Subdomain
            var tenant = await FindTenantBySubdomainAsync(request.Subdomain);
            if (tenant == null)
            {
                _logger.LogWarning("❌ Tenant không tồn tại: {Subdomain}", request.Subdomain);
                return LoginResponse.Fail("Subdomain không tồn tại hoặc đã bị vô hiệu hóa", "TENANT_NOT_FOUND");
            }

            // 2. Tạo scope mới với Tenant context
            using var scope = _serviceProvider.CreateScope();
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
            tenantService.SetTenant(tenant.Id, tenant.SchemaName);

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 3. Tìm User trong schema của Tenant
            var user = await dbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("❌ User không tồn tại: {Email}", request.Email);
                return LoginResponse.Fail("Email hoặc mật khẩu không đúng", "INVALID_CREDENTIALS");
            }

            // 4. Xác thực password với BCrypt
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("❌ Sai mật khẩu: {Email}", request.Email);
                return LoginResponse.Fail("Email hoặc mật khẩu không đúng", "INVALID_CREDENTIALS");
            }

            // 5. Lấy danh sách Roles và Permissions
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToList();

            // 6. Tạo JWT Access Token
            var accessToken = GenerateAccessToken(user, tenant, roles, permissions);

            // 7. Tạo và lưu Refresh Token
            var refreshToken = await CreateRefreshTokenAsync(dbContext, user.Id, ipAddress);

            // 8. Tạo response
            var tokens = new AuthTokens
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                RefreshTokenExpiresAt = refreshToken.ExpiresAt
            };

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                TenantId = tenant.Id,
                TenantName = tenant.Name,
                SchemaName = tenant.SchemaName,
                Roles = roles,
                Permissions = permissions
            };

            _logger.LogInformation("✅ Đăng nhập thành công: {Email} @ {Tenant}", user.Email, tenant.Name);

            return LoginResponse.Ok(tokens, userInfo);
        }

        /// <summary>
        /// Tìm Tenant từ Subdomain trong schema public.
        /// </summary>
        private async Task<Tenant?> FindTenantBySubdomainAsync(string subdomain)
        {
            var tenants = await _tenantRepository.GetAllAsync();
            return tenants.FirstOrDefault(t => 
                t.Subdomain.ToLower() == subdomain.ToLower() && t.Status == TenantStatus.Active);
        }

        #endregion

        #region Refresh Token

        /// <summary>
        /// Làm mới Access Token bằng Refresh Token.
        /// </summary>
        public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
        {
            _logger.LogInformation("🔄 Refresh token request");

            // Tìm refresh token trong tất cả các schemas
            // Lưu ý: RefreshToken nằm trong schema của từng tenant
            var (token, tenant) = await FindRefreshTokenAsync(refreshToken);

            if (token == null || tenant == null)
            {
                _logger.LogWarning("❌ Refresh token không hợp lệ");
                return RefreshTokenResponse.Fail("Refresh token không hợp lệ", "INVALID_TOKEN");
            }

            if (!token.IsActive)
            {
                _logger.LogWarning("❌ Refresh token đã hết hạn hoặc bị thu hồi");
                return RefreshTokenResponse.Fail("Refresh token đã hết hạn", "TOKEN_EXPIRED");
            }

            // Tạo scope với Tenant context
            using var scope = _serviceProvider.CreateScope();
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
            tenantService.SetTenant(tenant.Id, tenant.SchemaName);

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Lấy User
            var user = await dbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == token.UserId && u.IsActive);

            if (user == null)
            {
                return RefreshTokenResponse.Fail("User không còn hoạt động", "USER_INACTIVE");
            }

            // Thu hồi token cũ
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;

            // Tạo token mới
            var newRefreshToken = await CreateRefreshTokenAsync(dbContext, user.Id, ipAddress);
            token.ReplacedByToken = newRefreshToken.Token;

            await dbContext.SaveChangesAsync();

            // Tạo Access Token mới
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToList();

            var accessToken = GenerateAccessToken(user, tenant, roles, permissions);

            var tokens = new AuthTokens
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                RefreshTokenExpiresAt = newRefreshToken.ExpiresAt
            };

            _logger.LogInformation("✅ Token đã được làm mới cho: {Email}", user.Email);

            return RefreshTokenResponse.Ok(tokens);
        }

        /// <summary>
        /// Tìm RefreshToken trong tất cả các tenant schemas.
        /// </summary>
        private async Task<(RefreshToken? Token, Tenant? Tenant)> FindRefreshTokenAsync(string token)
        {
            // Lấy danh sách tất cả tenants
            var tenants = await _tenantRepository.GetAllAsync();

            foreach (var tenant in tenants.Where(t => t.Status == TenantStatus.Active))
            {
                using var scope = _serviceProvider.CreateScope();
                var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
                tenantService.SetTenant(tenant.Id, tenant.SchemaName);

                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var refreshToken = await dbContext.Set<RefreshToken>()
                    .FirstOrDefaultAsync(rt => rt.Token == token);

                if (refreshToken != null)
                {
                    return (refreshToken, tenant);
                }
            }

            return (null, null);
        }

        #endregion

        #region Revoke Token

        /// <summary>
        /// Thu hồi một Refresh Token.
        /// </summary>
        public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null)
        {
            var (token, tenant) = await FindRefreshTokenAsync(refreshToken);

            if (token == null || tenant == null || !token.IsActive)
            {
                return false;
            }

            using var scope = _serviceProvider.CreateScope();
            var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
            tenantService.SetTenant(tenant.Id, tenant.SchemaName);

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;

            dbContext.Set<RefreshToken>().Update(token);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("🚫 Token đã bị thu hồi");
            return true;
        }

        /// <summary>
        /// Thu hồi tất cả Refresh Tokens của một User.
        /// </summary>
        public async Task RevokeAllTokensAsync(Guid userId, string? ipAddress = null)
        {
            // Cần biết tenant của user - sẽ được implement sau khi có context
            _logger.LogWarning("RevokeAllTokensAsync chưa được implement đầy đủ");
        }

        #endregion

        #region Token Generation Helpers

        /// <summary>
        /// Tạo JWT Access Token với SaaS Claims.
        /// </summary>
        private string GenerateAccessToken(User user, Tenant tenant, List<string> roles, List<string> permissions)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                // Standard claims
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("name", user.FullName),

                // SaaS Claims (Quan trọng!)
                new Claim("tenant_id", tenant.Id.ToString()),
                new Claim("tenant_name", tenant.Name),
                new Claim("schema_name", tenant.SchemaName),
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add permission claims
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Tạo và lưu Refresh Token vào database.
        /// </summary>
        private async Task<RefreshToken> CreateRefreshTokenAsync(ApplicationDbContext dbContext, Guid userId, string? ipAddress)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = GenerateRandomToken(),
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                CreatedByIp = ipAddress,
                CreateAt = DateTime.UtcNow
            };

            dbContext.Set<RefreshToken>().Add(refreshToken);
            await dbContext.SaveChangesAsync();

            return refreshToken;
        }

        /// <summary>
        /// Tạo chuỗi random an toàn cho Refresh Token.
        /// </summary>
        private static string GenerateRandomToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        #endregion
    }
}
