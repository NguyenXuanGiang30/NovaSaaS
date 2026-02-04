using NovaSaaS.Application.Interfaces.AI;
using NovaSaaS.Domain.Entities.AI;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.Application.Interfaces;
using Pgvector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace NovaSaaS.Application.Services.AI
{
    /// <summary>
    /// RAGService - Orchestrate toàn bộ RAG Pipeline.
    /// 
    /// 1. Ingestion: Upload → Extract → Chunk → Embed → Store
    /// 2. Query: Question → Embed → Search → Context → LLM → Response
    /// </summary>
    public class RAGService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITextExtractionService _textExtractionService;
        private readonly IChunkingService _chunkingService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorSearchService _vectorSearchService;
        private readonly IChatCompletionService _chatService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IConfiguration _configuration;

        // RAG Settings
        private readonly int _chunkSize;
        private readonly int _chunkOverlap;
        private readonly int _topK;
        private readonly double _similarityThreshold;

        public RAGService(
            IUnitOfWork unitOfWork,
            ITextExtractionService textExtractionService,
            IChunkingService chunkingService,
            IEmbeddingService embeddingService,
            IVectorSearchService vectorSearchService,
            IChatCompletionService chatService,
            ICurrentUserService currentUserService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _textExtractionService = textExtractionService;
            _chunkingService = chunkingService;
            _embeddingService = embeddingService;
            _vectorSearchService = vectorSearchService;
            _chatService = chatService;
            _currentUserService = currentUserService;
            _configuration = configuration;

            // Load settings
            _chunkSize = configuration.GetValue("RAGSettings:ChunkSize", 800);
            _chunkOverlap = configuration.GetValue("RAGSettings:ChunkOverlap", 150);
            _topK = configuration.GetValue("RAGSettings:TopK", 5);
            _similarityThreshold = configuration.GetValue("RAGSettings:SimilarityThreshold", 0.7);
        }

        #region Ingestion Pipeline

        /// <summary>
        /// Xử lý tài liệu: Extract → Chunk → Embed → Store.
        /// </summary>
        public async Task<KnowledgeDocument> ProcessDocumentAsync(Guid documentId)
        {
            var document = await _unitOfWork.KnowledgeDocuments.GetByIdAsync(documentId);
            if (document == null)
                throw new ArgumentException("Document not found");

            try
            {
                // 1. Update status: Extracting
                document.Status = DocumentProcessingStatus.Extracting;
                _unitOfWork.KnowledgeDocuments.Update(document);
                await _unitOfWork.CompleteAsync();

                // 2. Extract text
                var filePath = document.FilePath;
                using var fileStream = File.OpenRead(filePath);
                var extension = Path.GetExtension(filePath);
                document.ExtractedContent = await _textExtractionService.ExtractTextAsync(fileStream, extension);

                // 3. Update status: Chunking
                document.Status = DocumentProcessingStatus.Chunking;
                _unitOfWork.KnowledgeDocuments.Update(document);
                await _unitOfWork.CompleteAsync();

                // 4. Chunk text
                var chunks = _chunkingService.ChunkText(document.ExtractedContent, _chunkSize, _chunkOverlap);

                // 5. Update status: Embedding
                document.Status = DocumentProcessingStatus.Embedding;
                _unitOfWork.KnowledgeDocuments.Update(document);
                await _unitOfWork.CompleteAsync();

                // 6. Generate embeddings (batch)
                var chunkTexts = chunks.Select(c => c.Content).ToList();
                var embeddings = await _embeddingService.GenerateEmbeddingsAsync(chunkTexts);

                // 7. Create segments
                var segments = new List<DocumentSegment>();
                for (int i = 0; i < chunks.Count; i++)
                {
                    segments.Add(new DocumentSegment
                    {
                        Id = Guid.NewGuid(),
                        DocumentId = documentId,
                        SegmentIndex = chunks[i].Index,
                        Content = chunks[i].Content,
                        Embedding = new Vector(embeddings[i]),
                        TokenCount = _chunkingService.EstimateTokenCount(chunks[i].Content),
                        StartPosition = chunks[i].StartPosition,
                        EndPosition = chunks[i].EndPosition,
                        CreatedBy = _currentUserService.UserId?.ToString()
                    });
                }

                // 8. Save segments
                _unitOfWork.DocumentSegments.AddRange(segments);

                // 9. Update document status
                document.Status = DocumentProcessingStatus.Completed;
                document.ProcessedAt = DateTime.UtcNow;
                document.SegmentCount = segments.Count;
                _unitOfWork.KnowledgeDocuments.Update(document);

                await _unitOfWork.CompleteAsync();

                return document;
            }
            catch (Exception ex)
            {
                document.Status = DocumentProcessingStatus.Failed;
                document.ErrorMessage = ex.Message;
                _unitOfWork.KnowledgeDocuments.Update(document);
                await _unitOfWork.CompleteAsync();
                throw;
            }
        }

        #endregion

        #region Query Pipeline

        /// <summary>
        /// Trả lời câu hỏi sử dụng RAG.
        /// </summary>
        public async Task<RAGResponse> AskAsync(string question)
        {
            var stopwatch = Stopwatch.StartNew();

            // 1. Tìm segments liên quan
            var searchResults = await _vectorSearchService.SearchByTextAsync(
                question, _topK, _similarityThreshold);

            // 2. Build context từ segments
            var context = BuildContext(searchResults);

            // 3. Build prompt
            var prompt = BuildPrompt(context, question);

            // 4. Generate response từ LLM
            var result = await _chatService.GenerateAsync(prompt);

            stopwatch.Stop();

            // 5. Lưu ChatHistory
            var chatHistory = new ChatHistory
            {
                Id = Guid.NewGuid(),
                UserId = Guid.TryParse(_currentUserService.UserId, out var uid) ? uid : Guid.Empty,
                Question = question,
                Answer = result.Content,
                RetrievedSegmentIds = JsonSerializer.Serialize(searchResults.Select(r => r.Segment.Id)),
                RetrievedCount = searchResults.Count,
                ConfidenceScore = searchResults.Any() ? (float)searchResults.Average(r => r.SimilarityScore) : 0,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                PromptTokens = result.PromptTokens,
                CompletionTokens = result.CompletionTokens,
                CreatedBy = _currentUserService.UserId?.ToString()
            };

            _unitOfWork.ChatHistories.Add(chatHistory);
            await _unitOfWork.CompleteAsync();

            return new RAGResponse
            {
                Answer = result.Content,
                Sources = searchResults.Select(r => new RAGSource
                {
                    SegmentId = r.Segment.Id,
                    DocumentId = r.Segment.DocumentId,
                    Content = r.Segment.Content,
                    SimilarityScore = r.SimilarityScore
                }).ToList(),
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                ConfidenceScore = chatHistory.ConfidenceScore ?? 0
            };
        }

        /// <summary>
        /// Xây dựng context từ các segments.
        /// </summary>
        private string BuildContext(List<VectorSearchResult> results)
        {
            if (!results.Any())
                return "Không tìm thấy thông tin liên quan trong tài liệu.";

            var contextParts = results.Select((r, index) =>
                $"[Nguồn {index + 1}]\n{r.Segment.Content}"
            );

            return string.Join("\n\n", contextParts);
        }

        /// <summary>
        /// Xây dựng prompt cho LLM.
        /// </summary>
        private string BuildPrompt(string context, string question)
        {
            return $@"Bạn là trợ lý AI thông minh của doanh nghiệp. Hãy trả lời câu hỏi dựa trên các thông tin được cung cấp bên dưới.

## Thông tin tham khảo:
{context}

## Câu hỏi của người dùng:
{question}

## Hướng dẫn:
1. Chỉ trả lời dựa trên thông tin trong tài liệu được cung cấp.
2. Nếu thông tin không có trong tài liệu, hãy nói rõ: ""Xin lỗi, tôi không tìm thấy thông tin này trong tài liệu hiện có.""
3. Trả lời ngắn gọn, chính xác và chuyên nghiệp.
4. Nếu có thể, trích dẫn nguồn bằng cách đề cập ""Theo nguồn X...""

## Câu trả lời:";
        }

        #endregion
    }

    /// <summary>
    /// Response từ RAG.
    /// </summary>
    public class RAGResponse
    {
        public string Answer { get; set; } = string.Empty;
        public List<RAGSource> Sources { get; set; } = new();
        public int ResponseTimeMs { get; set; }
        public float ConfidenceScore { get; set; }
    }

    /// <summary>
    /// Nguồn tham khảo trong response.
    /// </summary>
    public class RAGSource
    {
        public Guid SegmentId { get; set; }
        public Guid DocumentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
    }
}
