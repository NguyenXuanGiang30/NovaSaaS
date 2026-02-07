using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.PM
{
    /// <summary>
    /// ProjectComment - Bình luận trong task/dự án.
    /// </summary>
    public class ProjectComment : BaseEntity
    {
        /// <summary>
        /// ID task.
        /// </summary>
        public Guid ProjectTaskId { get; set; }
        public virtual ProjectTask ProjectTask { get; set; } = null!;

        /// <summary>
        /// ID người bình luận.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Nội dung bình luận.
        /// </summary>
        [Required]
        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// ID bình luận cha (reply).
        /// </summary>
        public Guid? ParentCommentId { get; set; }
        public virtual ProjectComment? ParentComment { get; set; }

        /// <summary>
        /// File đính kèm (URL).
        /// </summary>
        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        /// <summary>
        /// Đã chỉnh sửa.
        /// </summary>
        public bool IsEdited { get; set; } = false;
    }
}
