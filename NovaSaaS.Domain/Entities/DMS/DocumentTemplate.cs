using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.DMS
{
    /// <summary>
    /// DocumentTemplate - Mẫu tài liệu.
    /// </summary>
    public class DocumentTemplate : BaseEntity
    {
        /// <summary>
        /// Tên mẫu tài liệu.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Loại file.
        /// </summary>
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

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
        /// Module liên quan (chỉ hiện cho module cụ thể).
        /// </summary>
        [MaxLength(50)]
        public string? RelatedModule { get; set; }

        /// <summary>
        /// Thể loại template.
        /// </summary>
        [MaxLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Trạng thái hoạt động.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Người tạo.
        /// </summary>
        public Guid OwnerUserId { get; set; }
    }
}
