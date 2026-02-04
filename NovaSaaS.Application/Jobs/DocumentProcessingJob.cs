using NovaSaaS.Application.Interfaces.AI;
using NovaSaaS.Application.Services.AI;
using NovaSaaS.Domain.Entities.AI;
using NovaSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NovaSaaS.Application.Interfaces;

namespace NovaSaaS.Application.Jobs
{
    /// <summary>
    /// DocumentProcessingJob - Job xử lý tài liệu AI (chunking + embedding) trong background.
    /// Fire-and-forget khi upload document.
    /// </summary>
    public class DocumentProcessingJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChunkingService _chunkingService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DocumentProcessingJob> _logger;
        private readonly INotificationService _notificationService;

        public DocumentProcessingJob(
            IUnitOfWork unitOfWork,
            IChunkingService chunkingService,
            IEmbeddingService embeddingService,
            IConfiguration configuration,
            ILogger<DocumentProcessingJob> logger,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _chunkingService = chunkingService;
            _embeddingService = embeddingService;
            _configuration = configuration;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Xử lý document: chunking → embedding → lưu segments.
        /// </summary>
        /// <param name="tenantId">ID của tenant để gửi thông báo</param>
        /// <param name="documentId">ID của document cần xử lý</param>
        /// <param name="tenantSchemaName">Schema name của tenant để switch context</param>
        public async Task ProcessDocumentAsync(Guid tenantId, Guid documentId, string tenantSchemaName)
        {
            _logger.LogInformation("🔄 Starting document processing: {DocumentId} for Tenant {TenantId}", 
                documentId, tenantId);

            string fileName = "Unknown";

            try
            {
                // Note: Background Jobs cần đảm bảo DbContext đang connect đúng Schema.
                // Ở đây giả sử UnitOfWork/DbContext đã được cấu hình scope bởi Hangfire Activator 
                // hoặc middleware, nếu chưa thì cần set schema thủ công.
                // Tuy nhiên, logic này sẽ phụ thuộc vào implementation của DatabaseInitializer/TenantService.
                
                // Lấy document
                var document = await _unitOfWork.KnowledgeDocuments.GetByIdAsync(documentId);
                if (document == null)
                {
                    _logger.LogWarning("Document not found: {DocumentId}", documentId);
                    return;
                }

                fileName = document.FileName;

                // Notify Started
                await _notificationService.NotifyDocumentProcessingStartedAsync(tenantId, documentId, fileName);

                // Update status → Processing
                document.Status = DocumentProcessingStatus.Embedding;
                document.UpdateAt = DateTime.UtcNow;
                _unitOfWork.KnowledgeDocuments.Update(document);
                await _unitOfWork.CompleteAsync();

                // Lấy text content
                var textContent = document.ExtractedContent;
                if (string.IsNullOrEmpty(textContent))
                {
                    await _notificationService.NotifyDocumentProcessingFailedAsync(tenantId, documentId, fileName, "Document is empty");
                    return;
                }

                // Chunking
                var chunkSize = _configuration.GetValue<int>("RAGSettings:ChunkSize", 800);
                var chunkOverlap = _configuration.GetValue<int>("RAGSettings:ChunkOverlap", 150);
                var chunks = _chunkingService.ChunkText(textContent, chunkSize, chunkOverlap);

                _logger.LogInformation("📄 Document chunked: {ChunkCount} chunks", chunks.Count);

                // Tạo embeddings và lưu segments
                var segments = new List<DocumentSegment>();
                for (int i = 0; i < chunks.Count; i++)
                {
                    var chunk = chunks[i];
                    
                    try
                    {
                        var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);
                        
                        var segment = new DocumentSegment
                        {
                            Id = Guid.NewGuid(),
                            DocumentId = documentId,
                            SegmentIndex = i,
                            Content = chunk.Content,
                            Embedding = new Pgvector.Vector(embedding),
                            TokenCount = _chunkingService.EstimateTokenCount(chunk.Content),
                            StartPosition = chunk.StartPosition,
                            EndPosition = chunk.EndPosition,
                            CreateAt = DateTime.UtcNow
                        };

                        segments.Add(segment);
                        
                        // Notify Progress every 10 chunks or 20%
                        if (chunks.Count > 10 && (i + 1) % (chunks.Count / 5) == 0)
                        {
                            await _notificationService.NotifyDocumentProcessingProgressAsync(
                                tenantId, documentId, fileName, i + 1, chunks.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create embedding for chunk {Index}", i);
                    }
                }

                // Lưu tất cả segments
                foreach (var segment in segments)
                {
                    _unitOfWork.DocumentSegments.Add(segment);
                }

                // Update document status
                document.Status = DocumentProcessingStatus.Completed;
                document.SegmentCount = segments.Count;
                document.ProcessedAt = DateTime.UtcNow;
                document.UpdateAt = DateTime.UtcNow;
                _unitOfWork.KnowledgeDocuments.Update(document);

                await _unitOfWork.CompleteAsync();

                // Notify Completed
                await _notificationService.NotifyDocumentProcessingCompletedAsync(tenantId, documentId, fileName, segments.Count);

                _logger.LogInformation("✅ Document processed successfully: {DocumentId}", documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Document processing failed: {DocumentId}", documentId);
                
                await _notificationService.NotifyDocumentProcessingFailedAsync(tenantId, documentId, fileName, ex.Message);

                // Update DB status if possible
                try
                {
                    var document = await _unitOfWork.KnowledgeDocuments.GetByIdAsync(documentId);
                    if (document != null)
                    {
                        document.Status = DocumentProcessingStatus.Failed;
                        document.ErrorMessage = ex.Message;
                        _unitOfWork.KnowledgeDocuments.Update(document);
                        await _unitOfWork.CompleteAsync();
                    }
                }
                catch { /* Ignore */ }

                throw;
            }
        }
    }
}
