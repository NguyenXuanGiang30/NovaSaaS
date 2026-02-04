using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Master
{
    /// <summary>
    /// UsageLog - Tracking AI và resource consumption.
    /// Lưu trữ trong schema 'public' để Master Admin có thể xem usage của tất cả tenants.
    /// </summary>
    public class UsageLog : BaseEntity
    {
        /// <summary>
        /// Tenant đã sử dụng resource này.
        /// </summary>
        public Guid TenantId { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;

        /// <summary>
        /// Loại resource: AIChat, AIEmbedding, DocumentUpload, etc.
        /// </summary>
        public UsageType Type { get; set; }

        // ========================================
        // AI-specific fields
        // ========================================

        /// <summary>
        /// Số tokens trong prompt (input).
        /// </summary>
        public int? PromptTokens { get; set; }

        /// <summary>
        /// Số tokens trong completion (output).
        /// </summary>
        public int? CompletionTokens { get; set; }

        /// <summary>
        /// Tổng số tokens (prompt + completion).
        /// </summary>
        public int? TotalTokens => (PromptTokens ?? 0) + (CompletionTokens ?? 0);

        /// <summary>
        /// Chi phí ước tính (USD).
        /// Gemini 1.5 Flash: $0.075/1M input tokens, $0.30/1M output tokens
        /// </summary>
        public decimal? EstimatedCostUSD { get; set; }

        // ========================================
        // General fields
        // ========================================

        /// <summary>
        /// Mô tả hoặc metadata bổ sung.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Model AI được sử dụng (e.g., "gemini-1.5-flash").
        /// </summary>
        [MaxLength(50)]
        public string? ModelId { get; set; }

        /// <summary>
        /// User ID đã thực hiện action này.
        /// </summary>
        [MaxLength(50)]
        public string? UserId { get; set; }

        /// <summary>
        /// Kích thước file (bytes) - cho DocumentUpload.
        /// </summary>
        public long? FileSizeBytes { get; set; }

        /// <summary>
        /// Thời điểm ghi nhận usage.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
