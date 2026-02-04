using NovaSaaS.Domain.Entities.Common;
using Pgvector;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.AI
{
    /// <summary>
    /// DocumentSegment - Một đoạn văn bản từ tài liệu (chunk).
    /// Mỗi segment có embedding vector để tìm kiếm ngữ nghĩa.
    /// </summary>
    public class DocumentSegment : BaseEntity
    {
        /// <summary>
        /// ID tài liệu gốc.
        /// </summary>
        public Guid DocumentId { get; set; }
        public virtual KnowledgeDocument Document { get; set; } = null!;

        /// <summary>
        /// Thứ tự segment trong tài liệu (0-based).
        /// </summary>
        public int SegmentIndex { get; set; }

        /// <summary>
        /// Nội dung text của segment.
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Embedding vector (768 dimensions cho Gemini text-embedding-004).
        /// Sử dụng pgvector để lưu trữ và tìm kiếm.
        /// </summary>
        public Vector? Embedding { get; set; }

        /// <summary>
        /// Số lượng tokens trong segment.
        /// </summary>
        public int TokenCount { get; set; }

        /// <summary>
        /// Vị trí bắt đầu trong văn bản gốc.
        /// </summary>
        public int StartPosition { get; set; }

        /// <summary>
        /// Vị trí kết thúc trong văn bản gốc.
        /// </summary>
        public int EndPosition { get; set; }

        /// <summary>
        /// Metadata bổ sung (JSON).
        /// Ví dụ: số trang, heading...
        /// </summary>
        [MaxLength(1000)]
        public string? Metadata { get; set; }
    }
}
