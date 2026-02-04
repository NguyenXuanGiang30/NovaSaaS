using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Domain.Entities.Identity;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.WebApi.Authorization;
using NovaSaaS.WebApi.Models;

namespace NovaSaaS.WebApi.Controllers
{
    /// <summary>
    /// Roles Controller - Quản lý vai trò.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Tags("Identity")]
    public class RolesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            IUnitOfWork unitOfWork,
            ILogger<RolesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả roles.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<RoleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _unitOfWork.Roles.GetAllAsync();
            var result = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                PermissionCount = r.RolePermissions?.Count ?? 0,
                CreatedAt = r.CreateAt
            }).ToList();

            return Ok(ApiResponse<List<RoleDto>>.SuccessResult(result));
        }

        /// <summary>
        /// Lấy chi tiết role với permissions.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoleDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRole(Guid id)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id, r => r.RolePermissions);
            if (role == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy vai trò."));
            }

            var permissionIds = role.RolePermissions?.Select(rp => rp.PermissionId).ToList() ?? new List<Guid>();
            var permissions = await _unitOfWork.Permissions.FindAsync(p => permissionIds.Contains(p.Id));

            var result = new RoleDetailDto
            {
                Id = role.Id,
                Name = role.Name,
                CreatedAt = role.CreateAt,
                Permissions = permissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Description = p.Description
                }).ToList()
            };

            return Ok(ApiResponse<RoleDetailDto>.SuccessResult(result));
        }

        /// <summary>
        /// Tạo role mới.
        /// </summary>
        [HttpPost]
        [RequirePermission("roles.manage")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            // Check trùng tên
            var existing = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.Name == request.Name);
            if (existing != null)
            {
                return BadRequest(ApiResponse<object>.FailResult("Tên vai trò đã tồn tại."));
            }

            var role = new Role
            {
                Name = request.Name
            };

            _unitOfWork.Roles.Add(role);
            await _unitOfWork.CompleteAsync();

            // Add permissions if provided
            if (request.PermissionIds?.Any() == true)
            {
                foreach (var permissionId in request.PermissionIds)
                {
                    role.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId
                    });
                }
                await _unitOfWork.CompleteAsync();
            }

            _logger.LogInformation("Created role: {RoleName}", role.Name);

            return CreatedAtAction(nameof(GetRole), new { id = role.Id },
                ApiResponse<RoleDto>.SuccessResult(new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    PermissionCount = request.PermissionIds?.Count ?? 0,
                    CreatedAt = role.CreateAt
                }, "Đã tạo vai trò."));
        }

        /// <summary>
        /// Cập nhật role.
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission("roles.manage")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy vai trò."));
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                role.Name = request.Name;
            }

            _unitOfWork.Roles.Update(role);
            await _unitOfWork.CompleteAsync();

            return Ok(ApiResponse<RoleDto>.SuccessResult(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                CreatedAt = role.CreateAt
            }, "Đã cập nhật vai trò."));
        }

        /// <summary>
        /// Xóa role.
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission("roles.manage")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id, r => r.UserRoles);
            if (role == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy vai trò."));
            }

            if (role.UserRoles?.Any() == true)
            {
                return BadRequest(ApiResponse<object>.FailResult("Không thể xóa vai trò đang được gán cho người dùng."));
            }

            _unitOfWork.Roles.SoftDelete(role);
            await _unitOfWork.CompleteAsync();

            return Ok(ApiResponse<bool>.SuccessResult(true, "Đã xóa vai trò."));
        }

        /// <summary>
        /// Gán/Bỏ permissions cho role.
        /// </summary>
        [HttpPost("{id}/permissions")]
        [RequirePermission("roles.manage")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateRolePermissions(Guid id, [FromBody] UpdateRolePermissionsRequest request)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id, r => r.RolePermissions);
            if (role == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy vai trò."));
            }

            // Clear existing permissions
            role.RolePermissions.Clear();

            // Add new permissions
            foreach (var permissionId in request.PermissionIds)
            {
                role.RolePermissions.Add(new RolePermission
                {
                    RoleId = id,
                    PermissionId = permissionId
                });
            }

            _unitOfWork.Roles.Update(role);
            await _unitOfWork.CompleteAsync();

            return Ok(ApiResponse<bool>.SuccessResult(true, "Đã cập nhật quyền cho vai trò."));
        }

        /// <summary>
        /// Lấy danh sách người dùng được gán role này.
        /// </summary>
        [HttpGet("{id}/users")]
        [RequirePermission("roles.view")]
        [ProducesResponseType(typeof(ApiResponse<List<RoleUserDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleUsers(Guid id)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id, r => r.UserRoles);
            if (role == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy vai trò."));
            }

            if (role.UserRoles == null || !role.UserRoles.Any())
            {
                return Ok(ApiResponse<List<RoleUserDto>>.SuccessResult(new List<RoleUserDto>()));
            }

            var userIds = role.UserRoles.Select(ur => ur.UserId).ToList();
            var users = await _unitOfWork.Users.FindAsync(u => userIds.Contains(u.Id) && !u.IsDeleted);

            var result = users.Select(u => new RoleUserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                IsActive = u.IsActive
            }).ToList();

            return Ok(ApiResponse<List<RoleUserDto>>.SuccessResult(result));
        }

        /// <summary>
        /// Clone một role với tất cả permissions.
        /// </summary>
        [HttpPost("clone/{id}")]
        [RequirePermission("roles.manage")]
        [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CloneRole(Guid id, [FromBody] CloneRoleRequest request)
        {
            var sourceRole = await _unitOfWork.Roles.GetByIdAsync(id, r => r.RolePermissions);
            if (sourceRole == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy vai trò nguồn."));
            }

            // Check if new name already exists
            var existing = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.Name == request.NewName);
            if (existing != null)
            {
                return BadRequest(ApiResponse<object>.FailResult("Tên vai trò đã tồn tại."));
            }

            // Create new role
            var newRole = new Role
            {
                Name = request.NewName
            };
            _unitOfWork.Roles.Add(newRole);
            await _unitOfWork.CompleteAsync();

            // Clone permissions
            if (sourceRole.RolePermissions?.Any() == true)
            {
                foreach (var rp in sourceRole.RolePermissions)
                {
                    newRole.RolePermissions.Add(new RolePermission
                    {
                        RoleId = newRole.Id,
                        PermissionId = rp.PermissionId
                    });
                }
                await _unitOfWork.CompleteAsync();
            }

            _logger.LogInformation("Cloned role {SourceRole} to {NewRole}", sourceRole.Name, newRole.Name);

            return CreatedAtAction(nameof(GetRole), new { id = newRole.Id },
                ApiResponse<RoleDto>.SuccessResult(new RoleDto
                {
                    Id = newRole.Id,
                    Name = newRole.Name,
                    PermissionCount = sourceRole.RolePermissions?.Count ?? 0,
                    CreatedAt = newRole.CreateAt
                }, "Đã clone vai trò thành công."));
        }
    }

    #region DTOs

    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PermissionCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RoleDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public List<Guid>? PermissionIds { get; set; }
    }

    public class UpdateRoleRequest
    {
        public string? Name { get; set; }
    }

    public class UpdateRolePermissionsRequest
    {
        public List<Guid> PermissionIds { get; set; } = new();
    }

    public class RoleUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CloneRoleRequest
    {
        public string NewName { get; set; } = string.Empty;
    }

    #endregion
}
