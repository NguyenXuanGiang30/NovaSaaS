using NovaSaaS.Application.Interfaces.AI;
using NovaSaaS.Domain.Entities.AI;
using Pgvector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Interfaces.AI
{
    /// <summary>
    /// Kết quả tìm kiếm vector.
    /// </summary>
    public class VectorSearchResult
    {
        /// <summary>
        /// Segment được tìm thấy.
        /// </summary>
        public DocumentSegment Segment { get; set; } = null!;

        /// <summary>
        /// Điểm tương đồng (0-1, cao hơn = giống hơn).
        /// </summary>
        public double SimilarityScore { get; set; }

        /// <summary>
        /// Khoảng cách (thấp hơn = giống hơn).
        /// </summary>
        public double Distance { get; set; }
    }

    /// <summary>
    /// Interface cho Vector Search Service.
    /// Tìm kiếm ngữ nghĩa sử dụng pgvector.
    /// </summary>
    public interface IVectorSearchService
    {
        /// <summary>
        /// Tìm các segments tương đồng nhất với query.
        /// </summary>
        /// <param name="queryEmbedding">Embedding của câu query</param>
        /// <param name="topK">Số kết quả tối đa</param>
        /// <param name="similarityThreshold">Ngưỡng tương đồng tối thiểu (0-1)</param>
        /// <returns>Danh sách segments với điểm tương đồng</returns>
        Task<List<VectorSearchResult>> SearchAsync(
            float[] queryEmbedding,
            int topK = 5,
            double similarityThreshold = 0.7);

        /// <summary>
        /// Tìm kiếm với text query (tự động embed).
        /// </summary>
        Task<List<VectorSearchResult>> SearchByTextAsync(
            string queryText,
            int topK = 5,
            double similarityThreshold = 0.7);
    }
}
