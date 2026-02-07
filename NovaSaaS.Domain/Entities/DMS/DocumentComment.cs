using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.DMS
{
    /// <summary>
    /// DocumentComment - Bình luận trên tài liệu.
    /// </summary>
    public class DocumentComment : BaseEntity
    {
        /// <summary>
        /// ID tài liệu.
        /// </summary>
        public Guid DocumentId { get; set; }
        public virtual Document Document { get; set; } = null!;

        /// <summary>
        /// ID người bình luận.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Nội dung bình luận.
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// ID comment cha (reply).
        /// </summary>
        public Guid? ParentCommentId { get; set; }
        public virtual DocumentComment? ParentComment { get; set; }

        /// <summary>
        /// Đã chỉnh sửa.
        /// </summary>
        public bool IsEdited { get; set; } = false;
    }
}
