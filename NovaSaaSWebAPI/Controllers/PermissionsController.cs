using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.WebApi.Authorization;
using NovaSaaS.WebApi.Models;

namespace NovaSaaS.WebApi.Controllers
{
    /// <summary>
    /// Permissions Controller - Quản lý quyền.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Tags("Identity")]
    public class PermissionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(
            IUnitOfWork unitOfWork,
            ILogger<PermissionsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả permissions.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPermissions()
        {
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            var result = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Description = p.Description
            }).ToList();

            return Ok(ApiResponse<List<PermissionDto>>.SuccessResult(result));
        }

        /// <summary>
        /// Lấy chi tiết permission.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPermission(Guid id)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
            if (permission == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy quyền."));
            }

            return Ok(ApiResponse<PermissionDto>.SuccessResult(new PermissionDto
            {
                Id = permission.Id,
                Code = permission.Code,
                Description = permission.Description
            }));
        }

        /// <summary>
        /// Lấy permissions theo nhóm code.
        /// </summary>
        [HttpGet("by-prefix/{prefix}")]
        [ProducesResponseType(typeof(ApiResponse<List<PermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPermissionsByPrefix(string prefix)
        {
            var permissions = await _unitOfWork.Permissions.FindAsync(p => p.Code.StartsWith(prefix));
            var result = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Code = p.Code,
                Description = p.Description
            }).ToList();

            return Ok(ApiResponse<List<PermissionDto>>.SuccessResult(result));
        }
    }

    #region DTOs

    public class PermissionDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    #endregion
}
