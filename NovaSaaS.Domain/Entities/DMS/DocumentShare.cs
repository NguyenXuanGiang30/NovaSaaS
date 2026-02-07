using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.DMS
{
    /// <summary>
    /// DocumentShare - Chia sẻ tài liệu.
    /// </summary>
    public class DocumentShare : BaseEntity
    {
        /// <summary>
        /// ID tài liệu.
        /// </summary>
        public Guid DocumentId { get; set; }
        public virtual Document Document { get; set; } = null!;

        /// <summary>
        /// Loại chia sẻ.
        /// </summary>
        public ShareType ShareType { get; set; } = ShareType.User;

        /// <summary>
        /// ID đối tượng được chia sẻ (UserId, RoleId, DepartmentId...).
        /// </summary>
        public Guid? SharedWithId { get; set; }

        /// <summary>
        /// Quyền truy cập khi chia sẻ.
        /// </summary>
        public DocumentPermissionLevel PermissionLevel { get; set; } = DocumentPermissionLevel.View;

        /// <summary>
        /// Token chia sẻ công khai (nếu ShareType = Public).
        /// </summary>
        [MaxLength(100)]
        public string? ShareToken { get; set; }

        /// <summary>
        /// Mật khẩu bảo vệ (nếu có).
        /// </summary>
        [MaxLength(100)]
        public string? Password { get; set; }

        /// <summary>
        /// Ngày hết hạn chia sẻ.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Người chia sẻ.
        /// </summary>
        public Guid SharedByUserId { get; set; }

        /// <summary>
        /// Tin nhắn kèm theo.
        /// </summary>
        [MaxLength(500)]
        public string? Message { get; set; }

        /// <summary>
        /// Số lần truy cập.
        /// </summary>
        public int AccessCount { get; set; } = 0;
    }
}
