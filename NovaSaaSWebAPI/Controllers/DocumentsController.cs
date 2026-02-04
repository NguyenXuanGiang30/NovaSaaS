using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NovaSaaS.Application.Jobs;
using NovaSaaS.Application.Services.AI;
using NovaSaaS.Domain.Entities.AI;
using NovaSaaS.Domain.Interfaces;
using Hangfire;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaSWebAPI.Controllers
{
    /// <summary>
    /// API quản lý tài liệu Knowledge Base.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RAGService _ragService;
        private readonly IConfiguration _configuration;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ITenantService _tenantService;

        public DocumentsController(
            IUnitOfWork unitOfWork,
            RAGService ragService,
            IConfiguration configuration,
            IBackgroundJobClient backgroundJobClient,
            ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _ragService = ragService;
            _configuration = configuration;
            _backgroundJobClient = backgroundJobClient;
            _tenantService = tenantService;
        }

        /// <summary>
        /// Upload tài liệu mới.
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(52428800)] // 50MB
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string? description, [FromForm] string? tags)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file để upload");

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".docx", ".txt" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest($"Chỉ hỗ trợ các định dạng: {string.Join(", ", allowedExtensions)}");

            // Validate file size
            var maxSize = _configuration.GetValue<long>("FileStorage:MaxFileSizeMB", 50) * 1024 * 1024;
            if (file.Length > maxSize)
                return BadRequest($"File không được vượt quá {maxSize / 1024 / 1024}MB");

            // Save file
            var basePath = _configuration["FileStorage:BasePath"] ?? "uploads/documents";
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(basePath, fileName);

            Directory.CreateDirectory(basePath);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create document record
            var fileType = extension switch
            {
                ".pdf" => DocumentFileType.PDF,
                ".docx" => DocumentFileType.DOCX,
                ".txt" => DocumentFileType.TXT,
                _ => DocumentFileType.Unknown
            };

            var document = new KnowledgeDocument
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                FilePath = filePath,
                FileType = fileType,
                FileSize = file.Length,
                Description = description,
                Tags = tags,
                Status = DocumentProcessingStatus.Pending
            };

            _unitOfWork.KnowledgeDocuments.Add(document);
            await _unitOfWork.CompleteAsync();

            // Process document (đẩy vào Hangfire background job)
            var tenantId = _tenantService.TenantId ?? Guid.Empty; // Lưu ý: Cần đảm bảo TenantId có giá trị context
            var schemaName = _tenantService.SchemaName;

            _backgroundJobClient.Enqueue<DocumentProcessingJob>(job => 
                job.ProcessDocumentAsync(tenantId, document.Id, schemaName));

            return Ok(new
            {
                document.Id,
                document.FileName,
                document.Status,
                document.SegmentCount,
                Message = "Tài liệu đã được upload và đang xử lý"
            });
        }

        /// <summary>
        /// Lấy danh sách tài liệu.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var documents = await _unitOfWork.KnowledgeDocuments
                .Query()
                .Where(d => !d.IsDeleted)
                .Select(d => new
                {
                    d.Id,
                    d.FileName,
                    d.FileType,
                    d.FileSize,
                    d.Status,
                    d.SegmentCount,
                    d.Description,
                    d.Tags,
                    d.ProcessedAt,
                    d.CreateAt
                })
                .ToListAsync();

            return Ok(documents);
        }

        /// <summary>
        /// Lấy chi tiết tài liệu.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var document = await _unitOfWork.KnowledgeDocuments.GetByIdAsync(id);
            if (document == null)
                return NotFound("Không tìm thấy tài liệu");

            return Ok(new
            {
                document.Id,
                document.FileName,
                document.FileType,
                document.FileSize,
                document.Status,
                document.SegmentCount,
                document.Description,
                document.Tags,
                document.ErrorMessage,
                document.ProcessedAt,
                document.CreateAt
            });
        }

        /// <summary>
        /// Xóa tài liệu.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var document = await _unitOfWork.KnowledgeDocuments.GetByIdAsync(id);
            if (document == null)
                return NotFound("Không tìm thấy tài liệu");

            // Soft delete segments
            var segments = await _unitOfWork.DocumentSegments
                .Query()
                .Where(s => s.DocumentId == id)
                .ToListAsync();

            foreach (var segment in segments)
            {
                _unitOfWork.DocumentSegments.SoftDelete(segment);
            }

            // Soft delete document
            _unitOfWork.KnowledgeDocuments.SoftDelete(document);
            await _unitOfWork.CompleteAsync();

            // Optionally delete physical file
            if (System.IO.File.Exists(document.FilePath))
            {
                System.IO.File.Delete(document.FilePath);
            }

            return Ok(new { Message = "Đã xóa tài liệu thành công" });
        }

        /// <summary>
        /// Xử lý lại tài liệu.
        /// </summary>
        [HttpPost("{id}/reprocess")]
        public async Task<IActionResult> Reprocess(Guid id)
        {
            var document = await _unitOfWork.KnowledgeDocuments.GetByIdAsync(id);
            if (document == null)
                return NotFound("Không tìm thấy tài liệu");

            // Delete existing segments
            var segments = await _unitOfWork.DocumentSegments
                .Query()
                .Where(s => s.DocumentId == id)
                .ToListAsync();

            foreach (var segment in segments)
            {
                _unitOfWork.DocumentSegments.Remove(segment);
            }
            await _unitOfWork.CompleteAsync();

            // Reset status
            document.Status = DocumentProcessingStatus.Pending;
            document.ErrorMessage = null;
            document.SegmentCount = 0;
            _unitOfWork.KnowledgeDocuments.Update(document);
            await _unitOfWork.CompleteAsync();

            // Reprocess
            try
            {
                await _ragService.ProcessDocumentAsync(id);
                return Ok(new { Message = "Đã xử lý lại tài liệu thành công", document.SegmentCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Lỗi khi xử lý tài liệu", Error = ex.Message });
            }
        }
    }
}
