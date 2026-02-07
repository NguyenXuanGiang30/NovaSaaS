using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.DMS
{
    /// <summary>
    /// DocumentPermission - Phân quyền truy cập tài liệu/thư mục.
    /// </summary>
    public class DocumentPermission : BaseEntity
    {
        /// <summary>
        /// ID tài liệu (null nếu phân quyền cho folder).
        /// </summary>
        public Guid? DocumentId { get; set; }
        public virtual Document? Document { get; set; }

        /// <summary>
        /// ID thư mục (null nếu phân quyền cho document).
        /// </summary>
        public Guid? FolderId { get; set; }
        public virtual Folder? Folder { get; set; }

        /// <summary>
        /// ID User được phân quyền.
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// ID Role được phân quyền (thay thế User).
        /// </summary>
        public Guid? RoleId { get; set; }

        /// <summary>
        /// Cấp quyền.
        /// </summary>
        public DocumentPermissionLevel PermissionLevel { get; set; } = DocumentPermissionLevel.View;

        /// <summary>
        /// Quyền kế thừa từ folder cha.
        /// </summary>
        public bool IsInherited { get; set; } = false;

        /// <summary>
        /// Người cấp quyền.
        /// </summary>
        public Guid GrantedByUserId { get; set; }

        /// <summary>
        /// Ngày hết hạn quyền (null = vĩnh viễn).
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
