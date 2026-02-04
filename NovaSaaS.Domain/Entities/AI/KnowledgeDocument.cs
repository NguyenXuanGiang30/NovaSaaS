using NovaSaaS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.AI
{
    /// <summary>
    /// Trạng thái xử lý tài liệu.
    /// </summary>
    public enum DocumentProcessingStatus
    {
        /// <summary>
        /// Chờ xử lý.
        /// </summary>
        Pending,

        /// <summary>
        /// Đang trích xuất text.
        /// </summary>
        Extracting,

        /// <summary>
        /// Đang chia nhỏ (chunking).
        /// </summary>
        Chunking,

        /// <summary>
        /// Đang tạo embeddings.
        /// </summary>
        Embedding,

        /// <summary>
        /// Hoàn thành.
        /// </summary>
        Completed,

        /// <summary>
        /// Thất bại.
        /// </summary>
        Failed
    }

    /// <summary>
    /// Loại file tài liệu.
    /// </summary>
    public enum DocumentFileType
    {
        PDF,
        DOCX,
        TXT,
        Unknown
    }

    /// <summary>
    /// KnowledgeDocument - Tài liệu trong Knowledge Base của Tenant.
    /// Mỗi Tenant có thể upload nhiều tài liệu để AI tìm kiếm.
    /// </summary>
    public class KnowledgeDocument : BaseEntity
    {
        /// <summary>
        /// Tên file gốc.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Đường dẫn lưu trữ file (relative path).
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Loại file.
        /// </summary>
        public DocumentFileType FileType { get; set; } = DocumentFileType.Unknown;

        /// <summary>
        /// Kích thước file (bytes).
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Nội dung text đã trích xuất.
        /// </summary>
        public string? ExtractedContent { get; set; }

        /// <summary>
        /// Trạng thái xử lý.
        /// </summary>
        public DocumentProcessingStatus Status { get; set; } = DocumentProcessingStatus.Pending;

        /// <summary>
        /// Thời điểm hoàn thành xử lý.
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Thông báo lỗi (nếu có).
        /// </summary>
        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Số lượng segments đã tạo.
        /// </summary>
        public int SegmentCount { get; set; }

        /// <summary>
        /// Mô tả tài liệu (do user nhập).
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Tags để phân loại.
        /// </summary>
        [MaxLength(200)]
        public string? Tags { get; set; }

        // Navigation
        public virtual ICollection<DocumentSegment> Segments { get; set; } = new List<DocumentSegment>();
    }
}
