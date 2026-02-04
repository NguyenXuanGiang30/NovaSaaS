using System;

namespace NovaSaaS.Domain.Entities.Common
{
    /// <summary>
    /// Lớp cơ sở cho tất cả 27 entities trong hệ thống.
    /// Cung cấp các thuộc tính dùng chung và hỗ trợ Soft Delete.
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Primary Key - GUID.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Thời điểm tạo.
        /// </summary>
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời điểm cập nhật lần cuối.
        /// </summary>
        public DateTime? UpdateAt { get; set; }

        /// <summary>
        /// ID của người tạo.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// ID của người cập nhật lần cuối.
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Đã bị xóa mềm (Soft Delete).
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Thời điểm xóa mềm.
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}
