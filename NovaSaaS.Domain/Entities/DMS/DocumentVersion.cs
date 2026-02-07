using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.DMS
{
    /// <summary>
    /// DocumentVersion - Phiên bản tài liệu (versioning).
    /// </summary>
    public class DocumentVersion : BaseEntity
    {
        /// <summary>
        /// ID tài liệu.
        /// </summary>
        public Guid DocumentId { get; set; }
        public virtual Document Document { get; set; } = null!;

        /// <summary>
        /// Số phiên bản.
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Tên file.
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Dung lượng.
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Đường dẫn lưu trữ.
        /// </summary>
        [MaxLength(1000)]
        public string StoragePath { get; set; } = string.Empty;

        /// <summary>
        /// Loại file.
        /// </summary>
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Mã hash file (để detect thay đổi).
        /// </summary>
        [MaxLength(64)]
        public string? FileHash { get; set; }

        /// <summary>
        /// ID người upload.
        /// </summary>
        public Guid UploadedByUserId { get; set; }

        /// <summary>
        /// Ghi chú thay đổi (change log).
        /// </summary>
        [MaxLength(500)]
        public string? ChangeNotes { get; set; }
    }
}
