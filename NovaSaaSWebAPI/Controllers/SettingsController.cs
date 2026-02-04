using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.WebApi.Authorization;
using NovaSaaS.WebApi.Models;

namespace NovaSaaS.WebApi.Controllers
{
    /// <summary>
    /// Settings Controller - Quản lý cài đặt tenant.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Tags("Settings")]
    public class SettingsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            IUnitOfWork unitOfWork,
            ILogger<SettingsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Lấy cài đặt tenant hiện tại.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<TenantSettingDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _unitOfWork.TenantSettings.GetAllAsync();
            var setting = settings.FirstOrDefault();

            if (setting == null)
            {
                // Return default settings
                return Ok(ApiResponse<TenantSettingDto>.SuccessResult(new TenantSettingDto()));
            }

            return Ok(ApiResponse<TenantSettingDto>.SuccessResult(MapToDto(setting)));
        }

        /// <summary>
        /// Cập nhật cài đặt tenant.
        /// </summary>
        [HttpPut]
        [RequirePermission("settings.manage")]
        [ProducesResponseType(typeof(ApiResponse<TenantSettingDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
        {
            var settings = await _unitOfWork.TenantSettings.GetAllAsync();
            var setting = settings.FirstOrDefault();

            if (setting == null)
            {
                setting = new TenantSetting();
                _unitOfWork.TenantSettings.Add(setting);
            }

            if (request.PrimaryColor != null)
                setting.PrimaryColor = request.PrimaryColor;
            if (request.LogoUrl != null)
                setting.LogoUrl = request.LogoUrl;
            if (request.CompanyName != null)
                setting.CompanyName = request.CompanyName;
            if (request.Language != null)
                setting.Language = request.Language;

            _unitOfWork.TenantSettings.Update(setting);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Updated settings");

            return Ok(ApiResponse<TenantSettingDto>.SuccessResult(MapToDto(setting), "Đã cập nhật cài đặt."));
        }

        /// <summary>
        /// Reset cài đặt về mặc định.
        /// </summary>
        [HttpPost("reset")]
        [RequirePermission("settings.manage")]
        [ProducesResponseType(typeof(ApiResponse<TenantSettingDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetSettings()
        {
            var settings = await _unitOfWork.TenantSettings.GetAllAsync();
            var setting = settings.FirstOrDefault();

            if (setting != null)
            {
                setting.PrimaryColor = "#3b82f6";
                setting.LogoUrl = string.Empty;
                setting.Language = "vi";
                _unitOfWork.TenantSettings.Update(setting);
                await _unitOfWork.CompleteAsync();
                return Ok(ApiResponse<TenantSettingDto>.SuccessResult(MapToDto(setting), "Đã reset cài đặt về mặc định."));
            }
            
            return Ok(ApiResponse<TenantSettingDto>.SuccessResult(new TenantSettingDto(), "Đã reset cài đặt về mặc định."));
        }

        private TenantSettingDto MapToDto(TenantSetting setting)
        {
            return new TenantSettingDto
            {
                Id = setting.Id,
                PrimaryColor = setting.PrimaryColor,
                LogoUrl = setting.LogoUrl,
                CompanyName = setting.CompanyName,
                Language = setting.Language
            };
        }
    }

    #region DTOs

    public class TenantSettingDto
    {
        public Guid Id { get; set; }
        public string PrimaryColor { get; set; } = "#3b82f6";
        public string LogoUrl { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Language { get; set; } = "vi";
    }

    public class UpdateSettingsRequest
    {
        public string? PrimaryColor { get; set; }
        public string? LogoUrl { get; set; }
        public string? CompanyName { get; set; }
        public string? Language { get; set; }
    }

    #endregion
}
