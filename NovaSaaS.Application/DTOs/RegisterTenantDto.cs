using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Application.DTOs
{
    /// <summary>
    /// DTO cho việc đăng ký Tenant mới.
    /// </summary>
    public class RegisterTenantDto
    {
        /// <summary>
        /// Tên doanh nghiệp/tổ chức.
        /// </summary>
        [Required(ErrorMessage = "Tên doanh nghiệp là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên phải từ 2-100 ký tự")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Subdomain cho tenant (ví dụ: "apple" -> apple.novasaas.com).
        /// Chỉ chấp nhận chữ cái, số và dấu gạch ngang.
        /// </summary>
        [Required(ErrorMessage = "Subdomain là bắt buộc")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Subdomain phải từ 3-50 ký tự")]
        [RegularExpression(@"^[a-z0-9][a-z0-9-]*[a-z0-9]$", 
            ErrorMessage = "Subdomain chỉ được chứa chữ thường, số và dấu gạch ngang")]
        public string Subdomain { get; set; } = string.Empty;

        /// <summary>
        /// Email của Admin đầu tiên.
        /// </summary>
        [Required(ErrorMessage = "Email admin là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        public string AdminEmail { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu của Admin đầu tiên.
        /// </summary>
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8-100 ký tự")]
        public string AdminPassword { get; set; } = string.Empty;

        /// <summary>
        /// ID của gói cước (Basic, Pro, Enterprise).
        /// </summary>
        [Required(ErrorMessage = "Gói cước là bắt buộc")]
        public Guid PlanId { get; set; }

        /// <summary>
        /// Tên đầy đủ của Admin (tùy chọn).
        /// </summary>
        [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
        public string? AdminFullName { get; set; }

        /// <summary>
        /// Số điện thoại liên hệ (tùy chọn).
        /// </summary>
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }
    }

    /// <summary>
    /// Kết quả trả về khi đăng ký Tenant.
    /// </summary>
    public class RegistrationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public RegistrationDetails? Data { get; set; }

        public static RegistrationResult Ok(RegistrationDetails data, string? message = null)
        {
            return new RegistrationResult
            {
                Success = true,
                Message = message ?? "Đăng ký thành công",
                Data = data
            };
        }

        public static RegistrationResult Fail(string message, string errorCode)
        {
            return new RegistrationResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    /// <summary>
    /// Chi tiết thông tin sau khi đăng ký thành công.
    /// </summary>
    public class RegistrationDetails
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string LoginUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
