using Microsoft.EntityFrameworkCore;
using NovaSaaS.Application.Interfaces.AI;
using NovaSaaS.Domain.Interfaces;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Infrastructure.Services.AI
{
    /// <summary>
    /// VectorSearchService - Tìm kiếm ngữ nghĩa sử dụng pgvector.
    /// IMPLEMENTATION LAYER: Phụ thuộc vào EF Core và Pgvector.EntityFrameworkCore.
    /// 
    /// QUAN TRỌNG: Tất cả queries đều được filter theo TenantId thông qua DbContext.
    /// </summary>
    public class VectorSearchService : IVectorSearchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmbeddingService _embeddingService;

        public VectorSearchService(IUnitOfWork unitOfWork, IEmbeddingService embeddingService)
        {
            _unitOfWork = unitOfWork;
            _embeddingService = embeddingService;
        }

        /// <summary>
        /// Tìm các segments tương đồng nhất sử dụng Cosine Distance.
        /// </summary>
        public async Task<List<VectorSearchResult>> SearchAsync(
            float[] queryEmbedding,
            int topK = 5,
            double similarityThreshold = 0.7)
        {
            if (queryEmbedding == null || queryEmbedding.Length == 0)
                return new List<VectorSearchResult>();

            var queryVector = new Vector(queryEmbedding);

            // Query với pgvector Cosine Distance
            // Cosine Distance = 1 - Cosine Similarity
            // Nên: similarity = 1 - distance
            // Threshold 0.7 → max distance = 0.3
            var maxDistance = 1 - similarityThreshold;

            // Optimize HNSW search (ef_search > ef_construction ensures better recall)
            // Default is usually 40, increasing to 100 for better accuracy
            await _unitOfWork.ExecuteSqlRawAsync("SET hnsw.ef_search = 100");

            var results = await _unitOfWork.DocumentSegments
                .Query()
                .Where(s => s.Embedding != null && !s.IsDeleted)
                .Select(s => new
                {
                    Segment = s,
                    Distance = s.Embedding!.CosineDistance(queryVector)
                })
                .Where(x => x.Distance <= maxDistance)
                .OrderBy(x => x.Distance)
                .Take(topK)
                .ToListAsync();

            return results.Select(r => new VectorSearchResult
            {
                Segment = r.Segment,
                Distance = r.Distance,
                SimilarityScore = 1 - r.Distance
            }).ToList();
        }

        /// <summary>
        /// Tìm kiếm với text query (tự động embed).
        /// </summary>
        public async Task<List<VectorSearchResult>> SearchByTextAsync(
            string queryText,
            int topK = 5,
            double similarityThreshold = 0.7)
        {
            if (string.IsNullOrWhiteSpace(queryText))
                return new List<VectorSearchResult>();

            // Tạo embedding cho câu query
            var embedding = await _embeddingService.GenerateEmbeddingAsync(queryText);

            // Tìm kiếm
            return await SearchAsync(embedding, topK, similarityThreshold);
        }
    }
}
