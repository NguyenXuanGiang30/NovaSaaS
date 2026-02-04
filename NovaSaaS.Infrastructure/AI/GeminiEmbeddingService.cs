using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.Google;
using NovaSaaS.Application.Interfaces.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Infrastructure.AI
{
    /// <summary>
    /// GeminiEmbeddingService - Tạo embeddings sử dụng Google Gemini.
    /// 
    /// Model: text-embedding-004 (768 dimensions)
    /// </summary>
    public class GeminiEmbeddingService : IEmbeddingService
    {
        private readonly Kernel _kernel;
        private readonly ITextEmbeddingGenerationService _embeddingService;

        public GeminiEmbeddingService(IConfiguration configuration)
        {
            var apiKey = configuration["GeminiSettings:ApiKey"] 
                ?? throw new InvalidOperationException("GeminiSettings:ApiKey is required");
            var modelId = configuration["GeminiSettings:EmbeddingModel"] ?? "text-embedding-004";

            var builder = Kernel.CreateBuilder();
            
            // Thêm Gemini Embedding service
            builder.AddGoogleAIEmbeddingGeneration(
                modelId: modelId,
                apiKey: apiKey
            );

            _kernel = builder.Build();
            _embeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        }

        /// <summary>
        /// Tạo embedding cho một đoạn text.
        /// </summary>
        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<float>();

            try
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(text);
                return embedding.ToArray();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate embedding: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tạo embeddings cho nhiều đoạn text (batch).
        /// </summary>
        public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
        {
            if (texts == null || texts.Count == 0)
                return new List<float[]>();

            try
            {
                var embeddings = await _embeddingService.GenerateEmbeddingsAsync(texts);
                return embeddings.Select(e => e.ToArray()).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate embeddings: {ex.Message}", ex);
            }
        }
    }
}
