using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.Master
{
    /// <summary>
    /// SystemLog - Centralized logging cho tất cả errors và events.
    /// Lưu trữ trong schema 'public' để Master Admin theo dõi sức khỏe toàn hệ thống.
    /// </summary>
    public class SystemLog : BaseEntity
    {
        /// <summary>
        /// Tenant liên quan (null nếu là lỗi ở Master level).
        /// </summary>
        public Guid? TenantId { get; set; }
        public virtual Tenant? Tenant { get; set; }

        /// <summary>
        /// Mức độ nghiêm trọng.
        /// </summary>
        public SystemLogLevel Level { get; set; }

        /// <summary>
        /// Nguồn phát sinh log (Controller, Service, Middleware name).
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Thông điệp log.
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Exception type (e.g., "NullReferenceException").
        /// </summary>
        [MaxLength(200)]
        public string? ExceptionType { get; set; }

        /// <summary>
        /// Stack trace (nếu có exception).
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Request path gây ra lỗi.
        /// </summary>
        [MaxLength(500)]
        public string? RequestPath { get; set; }

        /// <summary>
        /// HTTP method (GET, POST, etc.).
        /// </summary>
        [MaxLength(10)]
        public string? HttpMethod { get; set; }

        /// <summary>
        /// User ID đã thực hiện request.
        /// </summary>
        [MaxLength(50)]
        public string? UserId { get; set; }

        /// <summary>
        /// IP address của client.
        /// </summary>
        [MaxLength(50)]
        public string? ClientIp { get; set; }

        /// <summary>
        /// Thời điểm ghi nhận log.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian xử lý request (ms).
        /// </summary>
        public long? DurationMs { get; set; }

        /// <summary>
        /// Dữ liệu bổ sung dạng JSON.
        /// </summary>
        public string? AdditionalData { get; set; }
    }
}
