using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.DTOs;
using NovaSaaS.Application.Interfaces;
using System.Threading.Tasks;

namespace NovaSaaSWebAPI.Controllers
{
    /// <summary>
    /// Controller cho việc đăng ký Tenant mới.
    /// Đây là điểm vào của quy trình Onboarding.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class RegistrationController : ControllerBase
    {
        private readonly ITenantRegistrationService _registrationService;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(
            ITenantRegistrationService registrationService,
            ILogger<RegistrationController> logger)
        {
            _registrationService = registrationService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng ký một Tenant mới.
        /// POST /api/registration/register
        /// </summary>
        /// <param name="dto">Thông tin đăng ký</param>
        /// <returns>Kết quả đăng ký với thông tin chi tiết</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegistrationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RegistrationResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterTenantDto dto)
        {
            // Validate ModelState (Data Annotations)
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(RegistrationResult.Fail(
                    string.Join("; ", errors),
                    "VALIDATION_FAILED"
                ));
            }

            _logger.LogInformation("📨 Nhận yêu cầu đăng ký từ: {Email} cho subdomain: {Subdomain}", 
                dto.AdminEmail, dto.Subdomain);

            var result = await _registrationService.RegisterAsync(dto);

            if (result.Success)
            {
                _logger.LogInformation("✅ Đăng ký thành công cho: {Subdomain}", dto.Subdomain);
                return Ok(result);
            }

            _logger.LogWarning("❌ Đăng ký thất bại: {Error} ({Code})", 
                result.Message, result.ErrorCode);
            
            return BadRequest(result);
        }

        /// <summary>
        /// Kiểm tra subdomain có khả dụng không.
        /// GET /api/registration/check-subdomain?subdomain=xxx
        /// </summary>
        [HttpGet("check-subdomain")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckSubdomain([FromQuery] string subdomain)
        {
            if (string.IsNullOrWhiteSpace(subdomain))
            {
                return Ok(new 
                { 
                    available = false, 
                    message = "Subdomain không được để trống" 
                });
            }

            var isAvailable = await _registrationService.IsSubdomainAvailableAsync(subdomain);

            return Ok(new
            {
                subdomain = subdomain.ToLowerInvariant(),
                available = isAvailable,
                message = isAvailable 
                    ? $"Subdomain '{subdomain}' có thể sử dụng" 
                    : $"Subdomain '{subdomain}' không khả dụng"
            });
        }
    }
}
