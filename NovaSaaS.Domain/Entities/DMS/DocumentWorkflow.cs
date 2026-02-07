using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.DMS
{
    /// <summary>
    /// DocumentWorkflow - Workflow phê duyệt tài liệu.
    /// </summary>
    public class DocumentWorkflow : BaseEntity
    {
        /// <summary>
        /// ID tài liệu.
        /// </summary>
        public Guid DocumentId { get; set; }
        public virtual Document Document { get; set; } = null!;

        /// <summary>
        /// Tên workflow.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái workflow.
        /// </summary>
        public DocumentWorkflowStatus Status { get; set; } = DocumentWorkflowStatus.Pending;

        /// <summary>
        /// Người khởi tạo workflow.
        /// </summary>
        public Guid InitiatedByUserId { get; set; }

        /// <summary>
        /// Ngày khởi tạo.
        /// </summary>
        public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày hoàn thành.
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Bước hiện tại.
        /// </summary>
        public int CurrentStep { get; set; } = 1;

        /// <summary>
        /// Ghi chú.
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<DocumentWorkflowStep> Steps { get; set; } = new List<DocumentWorkflowStep>();
    }

    /// <summary>
    /// DocumentWorkflowStep - Bước trong workflow phê duyệt.
    /// </summary>
    public class DocumentWorkflowStep : BaseEntity
    {
        /// <summary>
        /// ID workflow.
        /// </summary>
        public Guid DocumentWorkflowId { get; set; }
        public virtual DocumentWorkflow DocumentWorkflow { get; set; } = null!;

        /// <summary>
        /// Số thứ tự bước.
        /// </summary>
        public int StepNumber { get; set; }

        /// <summary>
        /// Tên bước.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// ID người duyệt.
        /// </summary>
        public Guid ReviewerUserId { get; set; }

        /// <summary>
        /// Hành động.
        /// </summary>
        public WorkflowStepAction? Action { get; set; }

        /// <summary>
        /// Nhận xét.
        /// </summary>
        [MaxLength(1000)]
        public string? Comment { get; set; }

        /// <summary>
        /// Ngày hành động.
        /// </summary>
        public DateTime? ActionDate { get; set; }

        /// <summary>
        /// Đã hoàn thành.
        /// </summary>
        public bool IsCompleted { get; set; } = false;
    }
}
