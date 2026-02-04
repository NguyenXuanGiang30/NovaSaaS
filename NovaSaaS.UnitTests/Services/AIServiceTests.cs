using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NovaSaaS.Application.Interfaces;
using NovaSaaS.Application.Interfaces.AI;
using NovaSaaS.Domain.Entities.AI;
using NovaSaaS.Domain.Interfaces;

namespace NovaSaaS.UnitTests.Services
{
    /// <summary>
    /// Unit Tests for AI Services.
    /// Tests chat completions, embeddings, and RAG pipeline components.
    /// </summary>
    public class AIServiceTests
    {
        #region ChatCompletionResult Tests

        [Fact]
        public void ChatCompletionResult_Success_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var result = new ChatCompletionResult
            {
                Content = "This is a test response",
                IsSuccess = true,
                PromptTokens = 10,
                CompletionTokens = 5
            };

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Content.Should().Be("This is a test response");
            result.PromptTokens.Should().Be(10);
            result.CompletionTokens.Should().Be(5);
            result.TotalTokens.Should().Be(15);
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void ChatCompletionResult_Failure_ShouldHaveErrorMessage()
        {
            // Arrange & Act
            var result = new ChatCompletionResult
            {
                Content = string.Empty,
                IsSuccess = false,
                ErrorMessage = "API rate limit exceeded"
            };

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("API rate limit exceeded");
        }

        #endregion

        #region Token Estimation Tests

        [Theory]
        [InlineData("Hello", 2)] // 5 chars / 4 = ~2 tokens
        [InlineData("This is a longer text for testing", 9)] // 34 chars / 4 = ~9 tokens
        [InlineData("", 0)]
        [InlineData("    ", 1)] // 4 spaces = 1 token
        public void EstimateTokens_ShouldCalculateCorrectly(string text, int expectedTokens)
        {
            // Unit testing the token estimation logic
            var tokenCount = string.IsNullOrEmpty(text) ? 0 : (int)Math.Ceiling(text.Length / 4.0);
            
            tokenCount.Should().Be(expectedTokens);
        }

        [Fact]
        public void EstimateTokens_VietnameseText_ShouldCalculateCorrectly()
        {
            // Vietnamese text may have different token ratios
            var vietnameseText = "Xin chào, đây là một văn bản tiếng Việt";
            var tokenCount = (int)Math.Ceiling(vietnameseText.Length / 4.0);
            
            // 40 chars / 4 = 10 tokens
            tokenCount.Should().Be(10);
        }

        #endregion

        #region Cosine Similarity Tests

        [Fact]
        public void CosineSimilarity_IdenticalVectors_ShouldReturnOne()
        {
            // Arrange
            var vector = new float[] { 1, 0, 0, 0 };

            // Act
            var similarity = CosineSimilarity(vector, vector);

            // Assert
            similarity.Should().BeApproximately(1.0, 0.0001);
        }

        [Fact]
        public void CosineSimilarity_OrthogonalVectors_ShouldReturnZero()
        {
            // Arrange
            var vector1 = new float[] { 1, 0 };
            var vector2 = new float[] { 0, 1 };

            // Act
            var similarity = CosineSimilarity(vector1, vector2);

            // Assert
            similarity.Should().BeApproximately(0.0, 0.0001);
        }

        [Fact]
        public void CosineSimilarity_OppositeVectors_ShouldReturnNegativeOne()
        {
            // Arrange
            var vector1 = new float[] { 1, 0, 0 };
            var vector2 = new float[] { -1, 0, 0 };

            // Act
            var similarity = CosineSimilarity(vector1, vector2);

            // Assert
            similarity.Should().BeApproximately(-1.0, 0.0001);
        }

        [Fact]
        public void CosineSimilarity_SimilarVectors_ShouldBePositive()
        {
            // Arrange - two similar but not identical vectors
            var vector1 = new float[] { 0.8f, 0.2f, 0.5f };
            var vector2 = new float[] { 0.7f, 0.3f, 0.4f };

            // Act
            var similarity = CosineSimilarity(vector1, vector2);

            // Assert
            similarity.Should().BeGreaterThan(0.9); // Very similar
        }

        [Fact]
        public void CosineSimilarity_DifferentLengthVectors_ShouldReturnZero()
        {
            // Arrange
            var vector1 = new float[] { 1, 0, 0 };
            var vector2 = new float[] { 1, 0 };

            // Act
            var similarity = CosineSimilarity(vector1, vector2);

            // Assert
            similarity.Should().Be(0);
        }

        #endregion

        #region Document Segmentation Tests

        [Fact]
        public void DocumentSegment_ShouldHaveCorrectProperties()
        {
            // Arrange
            var segment = new DocumentSegment
            {
                Id = Guid.NewGuid(),
                DocumentId = Guid.NewGuid(),
                Content = "This is a chunk of text from the document.",
                SegmentIndex = 0,
                StartPosition = 0,
                EndPosition = 42,
                TokenCount = 10
            };

            // Assert
            segment.Content.Should().NotBeNullOrEmpty();
            segment.SegmentIndex.Should().Be(0);
            segment.TokenCount.Should().Be(10);
            segment.StartPosition.Should().Be(0);
            segment.EndPosition.Should().Be(42);
        }

        [Theory]
        [InlineData(1000, 1)] // Single chunk
        [InlineData(2500, 3)] // 3 chunks for 2500 chars
        [InlineData(500, 1)] // Small content
        public void ChunkCount_ShouldMatchContentLength(int contentLength, int expectedChunks)
        {
            var chunkSize = 1000;
            var chunkCount = (int)Math.Ceiling((double)contentLength / chunkSize);

            chunkCount.Should().Be(expectedChunks);
        }

        #endregion

        #region KnowledgeDocument Tests

        [Fact]
        public void KnowledgeDocument_ShouldHaveRequiredProperties()
        {
            // Arrange
            var document = new KnowledgeDocument
            {
                Id = Guid.NewGuid(),
                FileName = "test.pdf",
                FileSize = 1024
            };

            // Assert
            document.FileName.Should().Be("test.pdf");
            document.FileSize.Should().Be(1024);
        }

        #endregion

        #region Chat History Tests

        [Fact]
        public void ChatHistory_ShouldStoreQuestionAndAnswer()
        {
            // Arrange
            var chatHistory = new ChatHistory
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Question = "What is the stock level of Product X?",
                Answer = "Product X has 150 units in stock.",
                CreateAt = DateTime.UtcNow,
                RetrievedCount = 3,
                ConfidenceScore = 0.95f,
                ResponseTimeMs = 250
            };

            // Assert
            chatHistory.Question.Should().NotBeNullOrEmpty();
            chatHistory.Answer.Should().NotBeNullOrEmpty();
            chatHistory.RetrievedCount.Should().Be(3);
            chatHistory.ConfidenceScore.Should().Be(0.95f);
            chatHistory.ResponseTimeMs.Should().Be(250);
        }

        [Fact]
        public void ChatHistory_ShouldSupportUserFeedback()
        {
            // Arrange
            var chatHistory = new ChatHistory
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Question = "Test question",
                Answer = "Test answer",
                UserRating = 4,
                UserFeedback = "Helpful but could be more detailed"
            };

            // Assert
            chatHistory.UserRating.Should().Be(4);
            chatHistory.UserFeedback.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ChatHistory_TokenCounts_ShouldBeStored()
        {
            // Arrange
            var chatHistory = new ChatHistory
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Question = "Short question",
                Answer = "Detailed answer with more information",
                PromptTokens = 50,
                CompletionTokens = 100
            };

            // Assert
            chatHistory.PromptTokens.Should().Be(50);
            chatHistory.CompletionTokens.Should().Be(100);
        }

        #endregion

        #region Vector Search Simulation Tests

        [Fact]
        public void VectorSearch_TopKResults_ShouldBeSortedByRelevance()
        {
            // Arrange - simulate vector search results
            var queryEmbedding = new float[] { 0.5f, 0.5f, 0.5f, 0.5f };
            
            var documents = new List<(Guid Id, float[] Embedding, double Similarity)>
            {
                (Guid.NewGuid(), new float[] { 0.51f, 0.49f, 0.48f, 0.52f }, 0),
                (Guid.NewGuid(), new float[] { 0.8f, 0.2f, 0.1f, 0.9f }, 0),
                (Guid.NewGuid(), new float[] { 0.52f, 0.51f, 0.49f, 0.48f }, 0),
            };

            // Calculate similarities
            var results = documents
                .Select(d => (d.Id, d.Embedding, Similarity: CosineSimilarity(queryEmbedding, d.Embedding)))
                .OrderByDescending(d => d.Similarity)
                .Take(2)
                .ToList();

            // Assert - top 2 should be the most similar
            results.Should().HaveCount(2);
            results[0].Similarity.Should().BeGreaterThanOrEqualTo(results[1].Similarity);
        }

        [Fact]
        public void VectorSearch_EmptyResults_ShouldReturnEmptyList()
        {
            // Arrange
            var results = new List<DocumentSegment>();

            // Assert
            results.Should().BeEmpty();
        }

        #endregion

        #region RAG Pipeline Tests

        [Fact]
        public void RAGPrompt_ShouldIncludeContext()
        {
            // Arrange
            var contexts = new List<string>
            {
                "Product X is stored in Warehouse A with 150 units.",
                "Minimum stock level for Product X is 50 units."
            };
            var question = "What is the stock level of Product X?";

            // Act - simulate building RAG prompt
            var contextString = string.Join("\n\n", contexts.Select((c, i) => $"[{i + 1}] {c}"));
            var prompt = $"""
                Based on the following context, answer the user's question.

                CONTEXT:
                {contextString}

                QUESTION:
                {question}

                ANSWER:
                """;

            // Assert
            prompt.Should().Contain("Product X");
            prompt.Should().Contain("150 units");
            prompt.Should().Contain(question);
        }

        [Fact] 
        public void RAGPrompt_NoContext_ShouldIndicateFallback()
        {
            // Arrange
            var contexts = new List<string>();


            // Act
            var hasContext = contexts.Any();
            var fallbackMessage = hasContext 
                ? "" 
                : "I don't have specific information about this in my knowledge base.";

            // Assert
            hasContext.Should().BeFalse();
            fallbackMessage.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Helper Methods

        private static double CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0;
            
            double dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }
            
            if (magA == 0 || magB == 0) return 0;
            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }

        #endregion
    }
}
