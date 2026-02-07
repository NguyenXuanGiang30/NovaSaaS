using NovaSaaS.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.PM
{
    /// <summary>
    /// TaskDependency - Quan hệ phụ thuộc giữa các task (Finish-to-Start).
    /// </summary>
    public class TaskDependency : BaseEntity
    {
        /// <summary>
        /// Task phụ thuộc (phải hoàn thành trước).
        /// </summary>
        public Guid PredecessorTaskId { get; set; }
        public virtual ProjectTask PredecessorTask { get; set; } = null!;

        /// <summary>
        /// Task bị phụ thuộc (chỉ bắt đầu khi predecessor xong).
        /// </summary>
        public Guid SuccessorTaskId { get; set; }
        public virtual ProjectTask SuccessorTask { get; set; } = null!;

        /// <summary>
        /// Loại dependency: FS (Finish-to-Start), SS, FF, SF.
        /// </summary>
        [MaxLength(5)]
        public string DependencyType { get; set; } = "FS";

        /// <summary>
        /// Số ngày lag (delay cho phép).
        /// </summary>
        public int LagDays { get; set; } = 0;
    }
}
