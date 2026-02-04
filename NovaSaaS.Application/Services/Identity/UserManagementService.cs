using NovaSaaS.Application.Interfaces;
using NovaSaaS.Domain.Entities.Identity;
using NovaSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace NovaSaaS.Application.Services.Identity
{
    /// <summary>
    /// UserManagementService - Quản lý người dùng và lời mời.
    /// Xử lý: mời user, accept invitation, manage roles.
    /// </summary>
    public class UserManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ICurrentUserService currentUserService,
            ILogger<UserManagementService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// Mời người dùng mới tham gia hệ thống.
        /// </summary>
        public async Task<InviteUserResult> InviteUserAsync(InviteUserRequest request)
        {
            // Check if email already exists
            var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return new InviteUserResult
                {
                    Success = false,
                    ErrorMessage = "Email này đã có tài khoản trong hệ thống."
                };
            }

            // Check if already invited
            var existingInvitation = await _unitOfWork.InvitationTokens.FirstOrDefaultAsync(
                i => i.Email == request.Email && i.Status == InvitationStatus.Pending);
            if (existingInvitation != null)
            {
                return new InviteUserResult
                {
                    Success = false,
                    ErrorMessage = "Email này đã được mời trước đó."
                };
            }

            Guid inviterId = Guid.Empty;
            if (!string.IsNullOrEmpty(_currentUserService.UserId) && 
                Guid.TryParse(_currentUserService.UserId, out var parsedId))
            {
                inviterId = parsedId;
            }

            var invitation = new InvitationToken
            {
                Email = request.Email,
                FullName = request.FullName,
                RoleId = request.RoleId,
                InvitedByUserId = inviterId,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _unitOfWork.InvitationTokens.Add(invitation);
            await _unitOfWork.CompleteAsync();

            // Send invitation email
            var inviteUrl = $"{request.BaseUrl}/accept-invitation?token={invitation.Token}";
            await _emailService.SendEmailAsync(new EmailMessage
            {
                To = request.Email,
                Subject = "Lời mời tham gia hệ thống",
                HtmlBody = $"<p>Bạn được mời tham gia hệ thống.</p><p><a href='{inviteUrl}'>Nhấn vào đây để tạo tài khoản</a></p>"
            });

            _logger.LogInformation("User invited: {Email} by {InviterId}", request.Email, inviterId);

            return new InviteUserResult
            {
                Success = true,
                InvitationId = invitation.Id,
                Token = invitation.Token
            };
        }

        /// <summary>
        /// Chấp nhận lời mời và tạo tài khoản.
        /// </summary>
        public async Task<AcceptInvitationResult> AcceptInvitationAsync(AcceptInvitationRequest request)
        {
            var invitation = await _unitOfWork.InvitationTokens.FirstOrDefaultAsync(
                i => i.Token == request.Token);

            if (invitation == null)
            {
                return new AcceptInvitationResult
                {
                    Success = false,
                    ErrorMessage = "Token không hợp lệ."
                };
            }

            if (!invitation.IsValid)
            {
                return new AcceptInvitationResult
                {
                    Success = false,
                    ErrorMessage = invitation.IsExpired
                        ? "Lời mời đã hết hạn."
                        : "Lời mời không còn hiệu lực."
                };
            }

            // Create user
            var user = new User
            {
                Email = invitation.Email,
                FullName = invitation.FullName ?? invitation.Email,
                PasswordHash = HashPassword(request.Password),
                IsActive = true
            };

            _unitOfWork.Users.Add(user);

            // Mark invitation as accepted
            invitation.Status = InvitationStatus.Accepted;
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.AcceptedUserId = user.Id;
            _unitOfWork.InvitationTokens.Update(invitation);

            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Invitation accepted: {Email} -> User {UserId}", invitation.Email, user.Id);

            return new AcceptInvitationResult
            {
                Success = true,
                UserId = user.Id
            };
        }

        /// <summary>
        /// Hủy lời mời.
        /// </summary>
        public async Task<bool> CancelInvitationAsync(Guid invitationId)
        {
            var invitation = await _unitOfWork.InvitationTokens.GetByIdAsync(invitationId);
            if (invitation == null || invitation.Status != InvitationStatus.Pending)
            {
                return false;
            }

            invitation.Status = InvitationStatus.Cancelled;
            _unitOfWork.InvitationTokens.Update(invitation);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Gửi lại lời mời.
        /// </summary>
        public async Task<InviteUserResult> ResendInvitationAsync(Guid invitationId, string baseUrl)
        {
            var invitation = await _unitOfWork.InvitationTokens.GetByIdAsync(invitationId);
            if (invitation == null)
            {
                return new InviteUserResult
                {
                    Success = false,
                    ErrorMessage = "Không tìm thấy lời mời."
                };
            }

            // Reset token and expiry
            invitation.Token = Guid.NewGuid().ToString();
            invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);
            invitation.Status = InvitationStatus.Pending;
            _unitOfWork.InvitationTokens.Update(invitation);
            await _unitOfWork.CompleteAsync();

            // Resend email
            var inviteUrl = $"{baseUrl}/accept-invitation?token={invitation.Token}";
            await _emailService.SendEmailAsync(new EmailMessage
            {
                To = invitation.Email,
                Subject = "Lời mời tham gia hệ thống (Gửi lại)",
                HtmlBody = $"<p>Bạn được mời tham gia hệ thống.</p><p><a href='{inviteUrl}'>Nhấn vào đây để tạo tài khoản</a></p>"
            });

            return new InviteUserResult
            {
                Success = true,
                InvitationId = invitation.Id,
                Token = invitation.Token
            };
        }

        /// <summary>
        /// Lấy danh sách người dùng.
        /// </summary>
        public async Task<List<UserDto>> GetUsersAsync()
        {
            var users = await _unitOfWork.Users.FindAsync(u => !u.IsDeleted);

            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                IsActive = u.IsActive,
                CreatedAt = u.CreateAt
            }).ToList();
        }

        /// <summary>
        /// Lấy danh sách lời mời đang chờ.
        /// </summary>
        public async Task<List<InvitationDto>> GetPendingInvitationsAsync()
        {
            var invitations = await _unitOfWork.InvitationTokens.FindAsync(
                i => i.Status == InvitationStatus.Pending);

            return invitations.Select(i => new InvitationDto
            {
                Id = i.Id,
                Email = i.Email,
                FullName = i.FullName,
                Status = i.Status.ToString(),
                ExpiresAt = i.ExpiresAt,
                CreatedAt = i.CreateAt
            }).ToList();
        }

        /// <summary>
        /// Kích hoạt/Vô hiệu hóa người dùng.
        /// </summary>
        public async Task<bool> SetUserStatusAsync(Guid userId, bool isActive)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = isActive;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Gán roles cho người dùng.
        /// </summary>
        public async Task<bool> AssignRolesAsync(Guid userId, List<Guid> roleIds)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, u => u.UserRoles);
            if (user == null) return false;

            // Clear existing roles
            user.UserRoles.Clear();

            // Add new roles
            foreach (var roleId in roleIds)
            {
                user.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });
            }

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        /// <summary>
        /// Lấy chi tiết người dùng.
        /// </summary>
        public async Task<UserDetailDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, u => u.UserRoles);
            if (user == null || user.IsDeleted) return null;

            // Get role details
            var roleIds = user.UserRoles?.Select(ur => ur.RoleId).ToList() ?? new List<Guid>();
            var roles = roleIds.Count > 0 
                ? await _unitOfWork.Roles.FindAsync(r => roleIds.Contains(r.Id))
                : Enumerable.Empty<Role>();

            return new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                CreatedAt = user.CreateAt,
                Roles = roles.Select(r => new RoleInfoDto
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            };
        }

        /// <summary>
        /// Cập nhật thông tin người dùng.
        /// </summary>
        public async Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserDto request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.IsDeleted) return null;

            if (request.FullName != null)
                user.FullName = request.FullName;

            user.UpdateAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User updated: {UserId}", userId);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                CreatedAt = user.CreateAt
            };
        }

        /// <summary>
        /// Xóa người dùng (soft delete).
        /// </summary>
        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || user.IsDeleted) return false;

            // Don't allow deleting self
            if (!string.IsNullOrEmpty(_currentUserService.UserId) &&
                Guid.TryParse(_currentUserService.UserId, out var currentId) &&
                currentId == userId)
            {
                return false;
            }

            _unitOfWork.Users.SoftDelete(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User deleted: {UserId}", userId);
            return true;
        }

        /// <summary>
        /// Lấy lịch sử hoạt động của người dùng.
        /// </summary>
        public async Task<List<UserActivityDto>> GetUserActivityAsync(Guid userId, int count = 50)
        {
            var auditLogs = await _unitOfWork.AuditLogs.FindAsync(
                a => a.UserId == userId.ToString());

            return auditLogs
                .OrderByDescending(a => a.CreateAt)
                .Take(count)
                .Select(a => new UserActivityDto
                {
                    Id = a.Id,
                    Action = a.Action,
                    EntityName = a.EntityName,
                    EntityId = a.EntityId,
                    Timestamp = a.CreateAt
                })
                .ToList();
        }

        private string HashPassword(string password)
        {
            // Simple hash for demo - production should use BCrypt or similar
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

    #region DTOs

    public class InviteUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public Guid? RoleId { get; set; }
        public string BaseUrl { get; set; } = string.Empty;
    }

    public class InviteUserResult
    {
        public bool Success { get; set; }
        public Guid InvitationId { get; set; }
        public string? Token { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class AcceptInvitationRequest
    {
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AcceptInvitationResult
    {
        public bool Success { get; set; }
        public Guid UserId { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class InvitationDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<RoleInfoDto> Roles { get; set; } = new();
    }

    public class RoleInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateUserDto
    {
        public string? FullName { get; set; }
    }

    public class UserActivityDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion
}
