using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.AI
{
    /// <summary>
    /// ChatHistory - Lịch sử hội thoại với AI.
    /// Lưu câu hỏi, câu trả lời, và các segments đã sử dụng.
    /// </summary>
    public class ChatHistory : BaseEntity
    {
        /// <summary>
        /// User đặt câu hỏi.
        /// </summary>
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Câu hỏi của user.
        /// </summary>
        [Required]
        public string Question { get; set; } = string.Empty;

        /// <summary>
        /// Câu trả lời từ AI.
        /// </summary>
        [Required]
        public string Answer { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách ID các segments đã sử dụng để trả lời (JSON array).
        /// </summary>
        public string? RetrievedSegmentIds { get; set; }

        /// <summary>
        /// Số lượng segments đã retrieve.
        /// </summary>
        public int RetrievedCount { get; set; }

        /// <summary>
        /// Điểm confidence của câu trả lời (0-1).
        /// </summary>
        public float? ConfidenceScore { get; set; }

        /// <summary>
        /// Thời gian xử lý (ms).
        /// </summary>
        public int? ResponseTimeMs { get; set; }

        /// <summary>
        /// Số tokens đã sử dụng cho prompt.
        /// </summary>
        public int? PromptTokens { get; set; }

        /// <summary>
        /// Số tokens trong response.
        /// </summary>
        public int? CompletionTokens { get; set; }

        /// <summary>
        /// User feedback (1-5 stars, null nếu chưa rate).
        /// </summary>
        public int? UserRating { get; set; }

        /// <summary>
        /// Feedback text từ user.
        /// </summary>
        [MaxLength(500)]
        public string? UserFeedback { get; set; }
    }
}
