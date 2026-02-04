using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.Services.Identity;
using NovaSaaS.WebApi.Authorization;
using NovaSaaS.WebApi.Models;

namespace NovaSaaS.WebApi.Controllers
{
    /// <summary>
    /// Users Controller - Quản lý người dùng trong tenant.
    /// API cho Team Management: invite, activate, role assignment.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Tags("Team Management")]
    public class UsersController : ControllerBase
    {
        private readonly UserManagementService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManagementService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách người dùng trong tenant.
        /// </summary>
        [HttpGet]
        [RequirePermission("users.view")]
        [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(ApiResponse<List<UserDto>>.SuccessResult(users));
        }

        /// <summary>
        /// Mời người dùng mới tham gia hệ thống.
        /// </summary>
        [HttpPost("invite")]
        [RequirePermission("users.invite")]
        [ProducesResponseType(typeof(ApiResponse<InviteUserResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InviteUser([FromBody] InviteUserApiRequest request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            
            var result = await _userService.InviteUserAsync(new InviteUserRequest
            {
                Email = request.Email,
                FullName = request.FullName,
                RoleId = request.RoleId,
                BaseUrl = baseUrl
            });

            if (!result.Success)
            {
                return BadRequest(ApiResponse<object>.FailResult(result.ErrorMessage!));
            }

            _logger.LogInformation("User invited: {Email}", request.Email);
            return Ok(ApiResponse<InviteUserResult>.SuccessResult(result, "Đã gửi lời mời."));
        }

        /// <summary>
        /// Xác nhận lời mời và tạo tài khoản.
        /// </summary>
        [HttpPost("accept-invitation")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AcceptInvitationResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationApiRequest request)
        {
            var result = await _userService.AcceptInvitationAsync(new AcceptInvitationRequest
            {
                Token = request.Token,
                Password = request.Password
            });

            if (!result.Success)
            {
                return BadRequest(ApiResponse<object>.FailResult(result.ErrorMessage!));
            }

            return Ok(ApiResponse<AcceptInvitationResult>.SuccessResult(result, "Tài khoản đã được tạo thành công."));
        }

        /// <summary>
        /// Lấy danh sách lời mời đang chờ xử lý.
        /// </summary>
        [HttpGet("invitations")]
        [RequirePermission("users.invite")]
        [ProducesResponseType(typeof(ApiResponse<List<InvitationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingInvitations()
        {
            var invitations = await _userService.GetPendingInvitationsAsync();
            return Ok(ApiResponse<List<InvitationDto>>.SuccessResult(invitations));
        }

        /// <summary>
        /// Hủy lời mời.
        /// </summary>
        [HttpDelete("invitations/{id}")]
        [RequirePermission("users.invite")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelInvitation(Guid id)
        {
            var result = await _userService.CancelInvitationAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy lời mời."));
            }

            return Ok(ApiResponse<bool>.SuccessResult(true, "Đã hủy lời mời."));
        }

        /// <summary>
        /// Gửi lại lời mời.
        /// </summary>
        [HttpPost("invitations/{id}/resend")]
        [RequirePermission("users.invite")]
        [ProducesResponseType(typeof(ApiResponse<InviteUserResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResendInvitation(Guid id)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _userService.ResendInvitationAsync(id, baseUrl);

            if (!result.Success)
            {
                return BadRequest(ApiResponse<object>.FailResult(result.ErrorMessage!));
            }

            return Ok(ApiResponse<InviteUserResult>.SuccessResult(result, "Đã gửi lại lời mời."));
        }

        /// <summary>
        /// Kích hoạt/Vô hiệu hóa người dùng.
        /// </summary>
        [HttpPut("{id}/status")]
        [RequirePermission("users.manage")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetUserStatus(Guid id, [FromBody] SetUserStatusRequest request)
        {
            var result = await _userService.SetUserStatusAsync(id, request.IsActive);
            if (!result)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy người dùng."));
            }

            var message = request.IsActive ? "Đã kích hoạt tài khoản." : "Đã vô hiệu hóa tài khoản.";
            return Ok(ApiResponse<bool>.SuccessResult(true, message));
        }

        /// <summary>
        /// Gán roles cho người dùng.
        /// </summary>
        [HttpPut("{id}/roles")]
        [RequirePermission("users.manage")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRolesRequest request)
        {
            var result = await _userService.AssignRolesAsync(id, request.RoleIds);
            if (!result)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy người dùng."));
            }

            return Ok(ApiResponse<bool>.SuccessResult(true, "Đã cập nhật roles."));
        }

        /// <summary>
        /// Lấy thông tin chi tiết người dùng.
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("users.view")]
        [ProducesResponseType(typeof(ApiResponse<UserDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy người dùng."));
            }

            return Ok(ApiResponse<UserDetailDto>.SuccessResult(user));
        }

        /// <summary>
        /// Cập nhật thông tin người dùng.
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("users.manage")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            var dto = new UpdateUserDto { FullName = request.FullName };
            var result = await _userService.UpdateUserAsync(id, dto);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy người dùng."));
            }

            _logger.LogInformation("User updated: {UserId}", id);
            return Ok(ApiResponse<UserDto>.SuccessResult(result, "Đã cập nhật thông tin."));
        }

        /// <summary>
        /// Xóa người dùng (soft delete).
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("users.manage")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy người dùng."));
            }

            _logger.LogInformation("User deleted: {UserId}", id);
            return Ok(ApiResponse<bool>.SuccessResult(true, "Đã xóa người dùng."));
        }

        /// <summary>
        /// Lấy lịch sử hoạt động của người dùng.
        /// </summary>
        [HttpGet("{id}/activity")]
        [RequirePermission("users.view")]
        [ProducesResponseType(typeof(ApiResponse<List<UserActivityDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserActivity(Guid id, [FromQuery] int count = 50)
        {
            var activities = await _userService.GetUserActivityAsync(id, count);
            return Ok(ApiResponse<List<UserActivityDto>>.SuccessResult(activities));
        }
    }

    #region API Request DTOs

    public class InviteUserApiRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public Guid? RoleId { get; set; }
    }

    public class AcceptInvitationApiRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SetUserStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class AssignRolesRequest
    {
        public List<Guid> RoleIds { get; set; } = new();
    }

    public class UpdateUserRequest
    {
        public string? FullName { get; set; }
    }

    #endregion
}
