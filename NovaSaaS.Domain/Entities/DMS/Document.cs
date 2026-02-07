using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.DMS
{
    /// <summary>
    /// Document - Tài liệu.
    /// </summary>
    public class Document : BaseEntity
    {
        /// <summary>
        /// Tên tài liệu.
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// ID thư mục chứa.
        /// </summary>
        public Guid FolderId { get; set; }
        public virtual Folder Folder { get; set; } = null!;

        /// <summary>
        /// Tên file gốc (upload).
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Loại file (MIME type: application/pdf, image/png...).
        /// </summary>
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Dung lượng (bytes).
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Đường dẫn lưu trữ (URL / storage path).
        /// </summary>
        [MaxLength(1000)]
        public string StoragePath { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái tài liệu.
        /// </summary>
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

        /// <summary>
        /// Phiên bản hiện tại.
        /// </summary>
        public int CurrentVersion { get; set; } = 1;

        /// <summary>
        /// ID người sở hữu.
        /// </summary>
        public Guid OwnerUserId { get; set; }

        /// <summary>
        /// Tags (JSON array hoặc comma-separated).
        /// </summary>
        [MaxLength(500)]
        public string? Tags { get; set; }

        /// <summary>
        /// Trạng thái check-out (ai đang lock).
        /// </summary>
        public Guid? CheckedOutByUserId { get; set; }

        /// <summary>
        /// Thời điểm check-out.
        /// </summary>
        public DateTime? CheckedOutAt { get; set; }

        /// <summary>
        /// Module liên quan (Project, HRM, ...).
        /// </summary>
        [MaxLength(50)]
        public string? RelatedModule { get; set; }

        /// <summary>
        /// ID entity liên quan.
        /// </summary>
        public Guid? RelatedEntityId { get; set; }

        /// <summary>
        /// Số lần tải xuống.
        /// </summary>
        public int DownloadCount { get; set; } = 0;

        // Navigation
        public virtual ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
        public virtual ICollection<DocumentComment> Comments { get; set; } = new List<DocumentComment>();
        public virtual ICollection<DocumentPermission> Permissions { get; set; } = new List<DocumentPermission>();
        public virtual ICollection<DocumentShare> Shares { get; set; } = new List<DocumentShare>();
        public virtual ICollection<DocumentWorkflow> Workflows { get; set; } = new List<DocumentWorkflow>();
    }
}
