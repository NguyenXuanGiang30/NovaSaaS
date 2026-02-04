using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using NovaSaaS.Application.Interfaces.AI;
using System;
using System.Threading.Tasks;
using SK = Microsoft.SemanticKernel.ChatCompletion;

namespace NovaSaaS.Infrastructure.AI
{
    /// <summary>
    /// GeminiChatService - Chat completion sử dụng Google Gemini.
    /// 
    /// Model: gemini-1.5-flash (hoặc gemini-pro)
    /// </summary>
    public class GeminiChatService : IChatCompletionService
    {
        private readonly Kernel _kernel;
        private readonly SK.IChatCompletionService _chatService;
        private readonly int _maxTokens;

        public GeminiChatService(IConfiguration configuration)
        {
            var apiKey = configuration["GeminiSettings:ApiKey"]
                ?? throw new InvalidOperationException("GeminiSettings:ApiKey is required");
            var modelId = configuration["GeminiSettings:ChatModel"] ?? "gemini-1.5-flash";
            _maxTokens = configuration.GetValue<int>("GeminiSettings:MaxTokens", 8192);

            var builder = Kernel.CreateBuilder();

            // Thêm Gemini Chat service
            builder.AddGoogleAIGeminiChatCompletion(
                modelId: modelId,
                apiKey: apiKey
            );

            _kernel = builder.Build();
            _kernel = builder.Build();
            _chatService = _kernel.GetRequiredService<SK.IChatCompletionService>();
        }

        /// <summary>
        /// Generate response từ prompt.
        /// </summary>
        public async Task<ChatCompletionResult> GenerateAsync(string prompt, int maxTokens = 2048)
        {
            try
            {
                var history = new SK.ChatHistory();
                history.AddUserMessage(prompt);

                var responses = await _chatService.GetChatMessageContentsAsync(
                    history,
                    new GeminiPromptExecutionSettings
                    {
                        MaxTokens = Math.Min(maxTokens, _maxTokens)
                    },
                    _kernel
                );

                var response = responses[0];

                return new ChatCompletionResult
                {
                    Content = response.Content ?? string.Empty,
                    IsSuccess = true,
                    // Token counts không được trả về trực tiếp từ Gemini connector
                    PromptTokens = EstimateTokens(prompt),
                    CompletionTokens = EstimateTokens(response.Content ?? "")
                };
            }
            catch (Exception ex)
            {
                return new ChatCompletionResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Generate response với system prompt.
        /// </summary>
        public async Task<ChatCompletionResult> GenerateAsync(string systemPrompt, string userMessage, int maxTokens = 2048)
        {
            try
            {
                var history = new SK.ChatHistory();
                history.AddSystemMessage(systemPrompt);
                history.AddUserMessage(userMessage);

                var responses = await _chatService.GetChatMessageContentsAsync(
                    history,
                    new GeminiPromptExecutionSettings
                    {
                        MaxTokens = Math.Min(maxTokens, _maxTokens)
                    },
                    _kernel
                );

                var response = responses[0];

                return new ChatCompletionResult
                {
                    Content = response.Content ?? string.Empty,
                    IsSuccess = true,
                    PromptTokens = EstimateTokens(systemPrompt + userMessage),
                    CompletionTokens = EstimateTokens(response.Content ?? "")
                };
            }
            catch (Exception ex)
            {
                return new ChatCompletionResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Ước tính số tokens.
        /// </summary>
        private int EstimateTokens(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
            // Ước tính: 1 token ≈ 4 ký tự
            return (int)Math.Ceiling(text.Length / 4.0);
        }
    }
}
