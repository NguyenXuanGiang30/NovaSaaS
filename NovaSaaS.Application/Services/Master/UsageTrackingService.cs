using NovaSaaS.Domain.Entities.Master;
using NovaSaaS.Domain.Enums;
using NovaSaaS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NovaSaaS.Application.Services.Master
{
    /// <summary>
    /// UsageTrackingService - Theo dõi và ghi nhận mức độ sử dụng tài nguyên (AI, Storage).
    /// Ghi vào bảng public.UsageLogs để Master Admin theo dõi chi phí.
    /// </summary>
    public class UsageTrackingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        // Gemini 1.5 Flash pricing (USD per 1M tokens)
        private const decimal InputPricePerMillionTokens = 0.075m;
        private const decimal OutputPricePerMillionTokens = 0.30m;

        public UsageTrackingService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        #region Logging Methods

        /// <summary>
        /// Ghi nhận usage từ AI Chat call.
        /// </summary>
        public async Task LogAIChatUsageAsync(
            Guid tenantId,
            int promptTokens,
            int completionTokens,
            string? modelId = null,
            string? description = null)
        {
            var estimatedCost = CalculateAICost(promptTokens, completionTokens);

            var log = new UsageLog
            {
                TenantId = tenantId,
                Type = UsageType.AIChat,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                EstimatedCostUSD = estimatedCost,
                ModelId = modelId ?? "gemini-1.5-flash",
                Description = description,
                UserId = _currentUser.UserId,
                Timestamp = DateTime.UtcNow,
                CreateAt = DateTime.UtcNow
            };

            _unitOfWork.UsageLogs.Add(log);
            await _unitOfWork.CompleteAsync();
        }

        /// <summary>
        /// Ghi nhận usage từ AI Embedding call.
        /// </summary>
        public async Task LogAIEmbeddingUsageAsync(
            Guid tenantId,
            int tokenCount,
            string? modelId = null,
            string? description = null)
        {
            var estimatedCost = CalculateEmbeddingCost(tokenCount);

            var log = new UsageLog
            {
                TenantId = tenantId,
                Type = UsageType.AIEmbedding,
                PromptTokens = tokenCount,
                EstimatedCostUSD = estimatedCost,
                ModelId = modelId ?? "text-embedding-004",
                Description = description,
                UserId = _currentUser.UserId,
                Timestamp = DateTime.UtcNow,
                CreateAt = DateTime.UtcNow
            };

            _unitOfWork.UsageLogs.Add(log);
            await _unitOfWork.CompleteAsync();
        }

        /// <summary>
        /// Ghi nhận document upload.
        /// </summary>
        public async Task LogDocumentUploadAsync(
            Guid tenantId,
            long fileSizeBytes,
            string? fileName = null)
        {
            var log = new UsageLog
            {
                TenantId = tenantId,
                Type = UsageType.DocumentUpload,
                FileSizeBytes = fileSizeBytes,
                Description = fileName,
                UserId = _currentUser.UserId,
                Timestamp = DateTime.UtcNow,
                CreateAt = DateTime.UtcNow
            };

            _unitOfWork.UsageLogs.Add(log);
            await _unitOfWork.CompleteAsync();
        }

        #endregion

        #region Reporting Methods

        /// <summary>
        /// Lấy tổng hợp usage của một Tenant trong tháng.
        /// </summary>
        public async Task<MonthlyUsageReportDto> GetMonthlyUsageAsync(Guid tenantId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            var logs = await _unitOfWork.UsageLogs.FindAsync(
                l => l.TenantId == tenantId && l.Timestamp >= startDate && l.Timestamp < endDate);

            var logList = logs.ToList();

            return new MonthlyUsageReportDto
            {
                TenantId = tenantId,
                Year = year,
                Month = month,
                TotalAIChatCalls = logList.Count(l => l.Type == UsageType.AIChat),
                TotalAIEmbeddingCalls = logList.Count(l => l.Type == UsageType.AIEmbedding),
                TotalPromptTokens = logList.Where(l => l.PromptTokens.HasValue).Sum(l => l.PromptTokens!.Value),
                TotalCompletionTokens = logList.Where(l => l.CompletionTokens.HasValue).Sum(l => l.CompletionTokens!.Value),
                TotalEstimatedCostUSD = logList.Where(l => l.EstimatedCostUSD.HasValue).Sum(l => l.EstimatedCostUSD!.Value),
                TotalDocumentUploads = logList.Count(l => l.Type == UsageType.DocumentUpload),
                TotalStorageBytes = logList.Where(l => l.FileSizeBytes.HasValue).Sum(l => l.FileSizeBytes!.Value)
            };
        }

        /// <summary>
        /// Lấy usage của tất cả Tenants (cho Master Admin).
        /// </summary>
        public async Task<IList<TenantUsageSummaryDto>> GetAllTenantsUsageAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            var logs = await _unitOfWork.UsageLogs.FindAsync(
                l => l.Timestamp >= startDate && l.Timestamp < endDate,
                l => l.Tenant);

            var grouped = logs
                .GroupBy(l => new { l.TenantId, l.Tenant.Name })
                .Select(g => new TenantUsageSummaryDto
                {
                    TenantId = g.Key.TenantId,
                    TenantName = g.Key.Name,
                    TotalAICalls = g.Count(l => l.Type == UsageType.AIChat || l.Type == UsageType.AIEmbedding),
                    TotalTokens = g.Where(l => l.PromptTokens.HasValue || l.CompletionTokens.HasValue)
                                   .Sum(l => (l.PromptTokens ?? 0) + (l.CompletionTokens ?? 0)),
                    TotalCostUSD = g.Where(l => l.EstimatedCostUSD.HasValue).Sum(l => l.EstimatedCostUSD!.Value),
                    TotalStorageMB = g.Where(l => l.FileSizeBytes.HasValue).Sum(l => l.FileSizeBytes!.Value) / 1024.0m / 1024.0m
                })
                .OrderByDescending(t => t.TotalCostUSD)
                .ToList();

            return grouped;
        }

        #endregion

        #region Quota Checking

        /// <summary>
        /// Kiểm tra quota AI còn lại của Tenant.
        /// </summary>
        public async Task<QuotaStatusDto> CheckAIQuotaAsync(Guid tenantId)
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                throw new ArgumentException("Tenant không tồn tại.");

            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(tenant.PlanId);
            var maxCalls = tenant.MaxMonthlyAICalls ?? plan?.MaxMonthlyAICalls ?? 100;

            var now = DateTime.UtcNow;
            var monthlyUsage = await GetMonthlyUsageAsync(tenantId, now.Year, now.Month);

            var usedCalls = monthlyUsage.TotalAIChatCalls + monthlyUsage.TotalAIEmbeddingCalls;
            var remaining = Math.Max(0, maxCalls - usedCalls);
            var percentUsed = maxCalls > 0 ? (decimal)usedCalls / maxCalls * 100 : 0;

            return new QuotaStatusDto
            {
                TenantId = tenantId,
                MaxAICalls = maxCalls,
                UsedAICalls = usedCalls,
                RemainingAICalls = remaining,
                PercentUsed = Math.Round(percentUsed, 2),
                IsQuotaExceeded = usedCalls >= maxCalls,
                AIEnabled = plan?.AIEnabled ?? false
            };
        }

        #endregion

        #region Cost Calculation

        private static decimal CalculateAICost(int promptTokens, int completionTokens)
        {
            var inputCost = (decimal)promptTokens / 1_000_000 * InputPricePerMillionTokens;
            var outputCost = (decimal)completionTokens / 1_000_000 * OutputPricePerMillionTokens;
            return Math.Round(inputCost + outputCost, 6);
        }

        private static decimal CalculateEmbeddingCost(int tokenCount)
        {
            // Embedding pricing: $0.00002 per 1K tokens (approximate)
            return Math.Round((decimal)tokenCount / 1000 * 0.00002m, 6);
        }

        #endregion
    }

    #region DTOs

    public class MonthlyUsageReportDto
    {
        public Guid TenantId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalAIChatCalls { get; set; }
        public int TotalAIEmbeddingCalls { get; set; }
        public int TotalPromptTokens { get; set; }
        public int TotalCompletionTokens { get; set; }
        public decimal TotalEstimatedCostUSD { get; set; }
        public int TotalDocumentUploads { get; set; }
        public long TotalStorageBytes { get; set; }

        public decimal TotalStorageMB => TotalStorageBytes / 1024m / 1024m;
        public int TotalTokens => TotalPromptTokens + TotalCompletionTokens;
    }

    public class TenantUsageSummaryDto
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public int TotalAICalls { get; set; }
        public int TotalTokens { get; set; }
        public decimal TotalCostUSD { get; set; }
        public decimal TotalStorageMB { get; set; }
    }

    public class QuotaStatusDto
    {
        public Guid TenantId { get; set; }
        public int MaxAICalls { get; set; }
        public int UsedAICalls { get; set; }
        public int RemainingAICalls { get; set; }
        public decimal PercentUsed { get; set; }
        public bool IsQuotaExceeded { get; set; }
        public bool AIEnabled { get; set; }
    }

    #endregion
}
