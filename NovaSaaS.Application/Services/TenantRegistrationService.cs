using Microsoft.Extensions.Logging;
using NovaSaaS.Application.DTOs;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Domain.Entities.Master;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services
{
    /// <summary>
    /// TenantRegistrationService - "Bộ phận điều hành và quản lý hợp đồng"
    /// 
    /// Đây là nơi chứa toàn bộ logic nghiệp vụ (Business Logic) để đưa 
    /// một khách hàng mới vào hệ thống một cách chính thức.
    /// 
    /// 4 Trách nhiệm:
    /// 1. Validation - Kiểm tra tính hợp lệ
    /// 2. Master Data Management - Ghi danh vào Sổ cái Master
    /// 3. Infrastructure Triggering - Ra lệnh khởi tạo hạ tầng
    /// 4. Super User Setup - Thiết lập người dùng tối cao
    /// </summary>
    public class TenantRegistrationService : ITenantRegistrationService
    {
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<SubscriptionPlan> _planRepository;
        private readonly IDatabaseInitializer _dbInitializer;
        private readonly ILogger<TenantRegistrationService> _logger;

        // Danh sách subdomain bị cấm
        private static readonly string[] ReservedSubdomains = new[]
        {
            "www", "api", "admin", "master", "app", "dashboard",
            "login", "register", "support", "help", "docs",
            "mail", "email", "ftp", "cdn", "static", "assets",
            "dev", "staging", "test", "demo", "beta", "alpha"
        };

        public TenantRegistrationService(
            IRepository<Tenant> tenantRepository,
            IRepository<SubscriptionPlan> planRepository,
            IDatabaseInitializer dbInitializer,
            ILogger<TenantRegistrationService> logger)
        {
            _tenantRepository = tenantRepository;
            _planRepository = planRepository;
            _dbInitializer = dbInitializer;
            _logger = logger;
        }

        /// <summary>
        /// Đăng ký một Tenant mới vào hệ thống.
        /// Quy trình: Validate → Create Master Record → Initialize Infrastructure
        /// </summary>
        public async Task<RegistrationResult> RegisterAsync(RegisterTenantDto dto)
        {
            _logger.LogInformation("🚀 Bắt đầu đăng ký Tenant: {Name} ({Subdomain})", dto.Name, dto.Subdomain);

            // ========================================
            // BƯỚC 1: VALIDATION (Kiểm tra tính hợp lệ)
            // ========================================
            var validationResult = await ValidateRegistrationAsync(dto);
            if (!validationResult.Success)
            {
                _logger.LogWarning("❌ Validation failed: {Error}", validationResult.Message);
                return validationResult;
            }

            // ========================================
            // BƯỚC 2: MASTER DATA MANAGEMENT
            // Ghi danh vào Sổ cái Master (public.Tenants)
            // ========================================
            var schemaName = GenerateSchemaName(dto.Subdomain);
            var now = DateTime.UtcNow;
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Subdomain = dto.Subdomain.ToLowerInvariant().Trim(),
                SchemaName = schemaName,
                PlanId = dto.PlanId,
                Status = TenantStatus.Provisioning,
                SubscriptionStartDate = now,
                SubscriptionEndDate = now.AddMonths(1), // 1 tháng trial mặc định
                CreateAt = now
            };

            _tenantRepository.Add(tenant);
            
            try
            {
                await _tenantRepository.SaveChangesAsync();
                _logger.LogInformation("📝 Đã tạo bản ghi Tenant: {TenantId} (Status: Provisioning)", tenant.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi tạo bản ghi Tenant");
                return RegistrationResult.Fail(
                    "Không thể tạo bản ghi Tenant. Vui lòng thử lại.",
                    "TENANT_CREATE_FAILED"
                );
            }

            // ========================================
            // BƯỚC 3: INFRASTRUCTURE TRIGGERING
            // Ra lệnh khởi tạo hạ tầng (Schema + 27 bảng + Seed data)
            // ========================================
            try
            {
                _logger.LogInformation("🏗️ Đang khởi tạo hạ tầng cho Schema: {Schema}", schemaName);
                
                await _dbInitializer.InitializeTenantAsync(
                    schemaName,
                    tenant.Id,
                    dto.AdminEmail,
                    dto.AdminPassword
                );

                _logger.LogInformation("✅ Hạ tầng đã được khởi tạo thành công");
            }
            catch (Exception ex)
            {
                // Rollback: Xóa bản ghi Tenant nếu khởi tạo thất bại
                _logger.LogError(ex, "❌ Lỗi khi khởi tạo hạ tầng, đang rollback...");
                
                await RollbackTenantAsync(tenant);
                
                return RegistrationResult.Fail(
                    "Không thể khởi tạo hạ tầng database. Vui lòng liên hệ hỗ trợ.",
                    "INFRASTRUCTURE_INIT_FAILED"
                );
            }

            // ========================================
            // BƯỚC 4: ACTIVATE TENANT
            // Cập nhật trạng thái thành Active
            // ========================================
            try
            {
                tenant.Status = TenantStatus.Active;
                tenant.UpdateAt = DateTime.UtcNow;
                await _tenantRepository.SaveChangesAsync();
                
                _logger.LogInformation("🎉 Tenant đã được kích hoạt: {TenantName}", tenant.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ Lỗi khi kích hoạt Tenant (Hạ tầng đã tạo xong)");
                // Không rollback ở đây vì hạ tầng đã tạo xong
            }

            // ========================================
            // BƯỚC 5: RETURN SUCCESS RESULT
            // ========================================
            var plan = await _planRepository.GetByIdAsync(dto.PlanId);
            
            var result = RegistrationResult.Ok(
                new RegistrationDetails
                {
                    TenantId = tenant.Id,
                    TenantName = tenant.Name,
                    Subdomain = tenant.Subdomain,
                    SchemaName = tenant.SchemaName,
                    AdminEmail = dto.AdminEmail,
                    PlanName = plan?.Name ?? "Unknown",
                    LoginUrl = $"https://{tenant.Subdomain}.novasaas.com",
                    CreatedAt = tenant.CreateAt
                },
                $"Chào mừng {tenant.Name} đến với NovaSaaS! Hệ thống của bạn đã sẵn sàng."
            );

            _logger.LogInformation("🎊 Đăng ký hoàn tất cho Tenant: {TenantName} ({TenantId})", 
                tenant.Name, tenant.Id);

            return result;
        }

        /// <summary>
        /// Kiểm tra subdomain có khả dụng không.
        /// </summary>
        public async Task<bool> IsSubdomainAvailableAsync(string subdomain)
        {
            if (string.IsNullOrWhiteSpace(subdomain))
                return false;

            var normalized = subdomain.ToLowerInvariant().Trim();

            // Kiểm tra subdomain bị cấm
            if (Array.Exists(ReservedSubdomains, s => s == normalized))
                return false;

            // Kiểm tra format
            if (!IsValidSubdomainFormat(normalized))
                return false;

            // Kiểm tra đã tồn tại chưa
            var exists = await _tenantRepository.AnyAsync(t => t.Subdomain == normalized);
            return !exists;
        }

        #region Validation Methods

        /// <summary>
        /// Validate toàn bộ dữ liệu đăng ký.
        /// </summary>
        private async Task<RegistrationResult> ValidateRegistrationAsync(RegisterTenantDto dto)
        {
            // 1. Validate Name
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length < 2)
            {
                return RegistrationResult.Fail(
                    "Tên doanh nghiệp phải có ít nhất 2 ký tự.",
                    "INVALID_NAME"
                );
            }

            // 2. Validate Subdomain format
            if (!IsValidSubdomainFormat(dto.Subdomain))
            {
                return RegistrationResult.Fail(
                    "Subdomain không hợp lệ. Chỉ được chứa chữ thường, số và dấu gạch ngang.",
                    "INVALID_SUBDOMAIN_FORMAT"
                );
            }

            // 3. Validate Subdomain không bị cấm
            if (Array.Exists(ReservedSubdomains, s => s == dto.Subdomain.ToLowerInvariant()))
            {
                return RegistrationResult.Fail(
                    $"Subdomain '{dto.Subdomain}' đã được hệ thống sử dụng. Vui lòng chọn tên khác.",
                    "SUBDOMAIN_RESERVED"
                );
            }

            // 4. Validate Subdomain chưa tồn tại
            var subdomainExists = await _tenantRepository.AnyAsync(
                t => t.Subdomain == dto.Subdomain.ToLowerInvariant()
            );
            if (subdomainExists)
            {
                return RegistrationResult.Fail(
                    $"Subdomain '{dto.Subdomain}' đã được đăng ký. Vui lòng chọn tên khác.",
                    "SUBDOMAIN_TAKEN"
                );
            }

            // 5. Validate Email format
            if (!IsValidEmail(dto.AdminEmail))
            {
                return RegistrationResult.Fail(
                    "Email không hợp lệ.",
                    "INVALID_EMAIL"
                );
            }

            // 6. Validate Password strength
            if (string.IsNullOrWhiteSpace(dto.AdminPassword) || dto.AdminPassword.Length < 8)
            {
                return RegistrationResult.Fail(
                    "Mật khẩu phải có ít nhất 8 ký tự.",
                    "WEAK_PASSWORD"
                );
            }

            // 7. Validate Plan exists
            var planExists = await _planRepository.AnyAsync(p => p.Id == dto.PlanId);
            if (!planExists)
            {
                return RegistrationResult.Fail(
                    "Gói cước không tồn tại. Vui lòng chọn gói cước hợp lệ.",
                    "INVALID_PLAN"
                );
            }

            return RegistrationResult.Ok(null!, "Validation passed");
        }

        private static bool IsValidSubdomainFormat(string subdomain)
        {
            if (string.IsNullOrWhiteSpace(subdomain))
                return false;

            // 3-50 ký tự, chỉ chứa chữ thường, số, dấu gạch ngang
            // Không bắt đầu hoặc kết thúc bằng dấu gạch ngang
            var pattern = @"^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$";
            return Regex.IsMatch(subdomain.ToLowerInvariant(), pattern);
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generate schema name từ subdomain.
        /// Ví dụ: "my-company" -> "tenant_my_company"
        /// </summary>
        private static string GenerateSchemaName(string subdomain)
        {
            var sanitized = subdomain
                .ToLowerInvariant()
                .Trim()
                .Replace("-", "_");

            // Loại bỏ ký tự không hợp lệ
            sanitized = Regex.Replace(sanitized, @"[^a-z0-9_]", "");

            return $"tenant_{sanitized}";
        }

        /// <summary>
        /// Rollback bản ghi Tenant khi khởi tạo thất bại.
        /// </summary>
        private async Task RollbackTenantAsync(Tenant tenant)
        {
            try
            {
                _tenantRepository.Remove(tenant);
                await _tenantRepository.SaveChangesAsync();
                _logger.LogInformation("🧹 Đã rollback bản ghi Tenant: {TenantId}", tenant.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ Không thể rollback bản ghi Tenant: {TenantId}", tenant.Id);
            }
        }

        #endregion
    }
}
