using NovaSaaS.Domain.Entities.Common;
using NovaSaaS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NovaSaaS.Domain.Entities.DMS
{
    /// <summary>
    /// Folder - Thư mục tài liệu (hỗ trợ cây phân cấp).
    /// </summary>
    public class Folder : BaseEntity
    {
        /// <summary>
        /// Tên thư mục.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả.
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// ID thư mục cha (null = root).
        /// </summary>
        public Guid? ParentFolderId { get; set; }
        public virtual Folder? ParentFolder { get; set; }

        /// <summary>
        /// Loại thư mục.
        /// </summary>
        public FolderType FolderType { get; set; } = FolderType.General;

        /// <summary>
        /// Đường dẫn đầy đủ (cache).
        /// </summary>
        [MaxLength(1000)]
        public string? FullPath { get; set; }

        /// <summary>
        /// Thứ tự hiển thị.
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Màu sắc (hex).
        /// </summary>
        [MaxLength(10)]
        public string? Color { get; set; }

        /// <summary>
        /// ID người sở hữu.
        /// </summary>
        public Guid OwnerUserId { get; set; }

        // Navigation
        public virtual ICollection<Folder> ChildFolders { get; set; } = new List<Folder>();
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<DocumentPermission> Permissions { get; set; } = new List<DocumentPermission>();
    }
}
