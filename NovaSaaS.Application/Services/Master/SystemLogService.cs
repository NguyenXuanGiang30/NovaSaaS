using NovaSaaS.Application.DTOs.Business;
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
    /// SystemLogService - Centralized logging cho toàn hệ thống.
    /// Ghi vào bảng public.SystemLogs để Master Admin theo dõi sức khỏe hệ thống.
    /// </summary>
    public class SystemLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ITenantService _tenantService;

        public SystemLogService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _tenantService = tenantService;
        }

        #region Logging Methods

        /// <summary>
        /// Ghi log với đầy đủ thông tin.
        /// </summary>
        public async Task LogAsync(
            Guid? tenantId,
            SystemLogLevel level,
            string source,
            string message,
            Exception? exception = null,
            string? requestPath = null,
            string? httpMethod = null,
            string? clientIp = null,
            long? durationMs = null,
            string? additionalData = null)
        {
            var log = new SystemLog
            {
                TenantId = tenantId ?? _tenantService.TenantId,
                Level = level,
                Source = source,
                Message = message,
                ExceptionType = exception?.GetType().Name,
                StackTrace = exception?.StackTrace,
                RequestPath = requestPath,
                HttpMethod = httpMethod,
                UserId = _currentUser.UserId,
                ClientIp = clientIp,
                Timestamp = DateTime.UtcNow,
                DurationMs = durationMs,
                AdditionalData = additionalData,
                CreateAt = DateTime.UtcNow
            };

            _unitOfWork.SystemLogs.Add(log);
            await _unitOfWork.CompleteAsync();
        }

        /// <summary>
        /// Shortcut: Log error.
        /// </summary>
        public async Task LogErrorAsync(string source, string message, Exception? exception = null)
        {
            await LogAsync(null, SystemLogLevel.Error, source, message, exception);
        }

        /// <summary>
        /// Shortcut: Log warning.
        /// </summary>
        public async Task LogWarningAsync(string source, string message)
        {
            await LogAsync(null, SystemLogLevel.Warning, source, message);
        }

        /// <summary>
        /// Shortcut: Log info.
        /// </summary>
        public async Task LogInfoAsync(string source, string message)
        {
            await LogAsync(null, SystemLogLevel.Info, source, message);
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Lấy logs với phân trang và lọc.
        /// </summary>
        public async Task<PaginatedResult<SystemLogDto>> GetLogsAsync(SystemLogFilterDto filter)
        {
            var allLogs = await _unitOfWork.SystemLogs.FindAsync(
                _ => true,
                l => l.Tenant!);

            var query = allLogs.AsQueryable();

            // Apply filters
            if (filter.TenantId.HasValue)
                query = query.Where(l => l.TenantId == filter.TenantId.Value);

            if (filter.Level.HasValue)
                query = query.Where(l => l.Level == filter.Level.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(l => l.Timestamp >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(l => l.Timestamp <= filter.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.Source))
                query = query.Where(l => l.Source.Contains(filter.Source));

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(l => 
                    l.Message.Contains(filter.SearchTerm) || 
                    (l.ExceptionType != null && l.ExceptionType.Contains(filter.SearchTerm)));

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(l => l.Timestamp)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => MapToDto(l))
                .ToList();

            return new PaginatedResult<SystemLogDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        /// <summary>
        /// Lấy health summary của hệ thống.
        /// </summary>
        public async Task<SystemHealthDto> GetSystemHealthAsync()
        {
            var now = DateTime.UtcNow;
            var last24h = now.AddHours(-24);
            var lastHour = now.AddHours(-1);

            var recentLogs = await _unitOfWork.SystemLogs.FindAsync(
                l => l.Timestamp >= last24h);

            var logList = recentLogs.ToList();
            var lastHourLogs = logList.Where(l => l.Timestamp >= lastHour).ToList();

            return new SystemHealthDto
            {
                Timestamp = now,
                TotalLogsLast24h = logList.Count,
                ErrorsLast24h = logList.Count(l => l.Level == SystemLogLevel.Error),
                CriticalsLast24h = logList.Count(l => l.Level == SystemLogLevel.Critical),
                WarningsLast24h = logList.Count(l => l.Level == SystemLogLevel.Warning),
                ErrorsLastHour = lastHourLogs.Count(l => l.Level >= SystemLogLevel.Error),
                MostCommonErrors = logList
                    .Where(l => l.Level >= SystemLogLevel.Error)
                    .GroupBy(l => l.ExceptionType ?? l.Message.Substring(0, Math.Min(100, l.Message.Length)))
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => new ErrorSummary { Error = g.Key ?? "Unknown", Count = g.Count() })
                    .ToList(),
                TenantErrorCounts = logList
                    .Where(l => l.Level >= SystemLogLevel.Error && l.TenantId.HasValue)
                    .GroupBy(l => l.TenantId!.Value)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => new TenantErrorCount { TenantId = g.Key, ErrorCount = g.Count() })
                    .ToList()
            };
        }

        /// <summary>
        /// Lấy tenant health summary.
        /// </summary>
        public async Task<IList<TenantHealthSummaryDto>> GetTenantsHealthAsync()
        {
            var last24h = DateTime.UtcNow.AddHours(-24);

            var logs = await _unitOfWork.SystemLogs.FindAsync(
                l => l.Timestamp >= last24h && l.TenantId.HasValue,
                l => l.Tenant!);

            var grouped = logs
                .GroupBy(l => new { l.TenantId, TenantName = l.Tenant?.Name ?? "Unknown" })
                .Select(g => new TenantHealthSummaryDto
                {
                    TenantId = g.Key.TenantId!.Value,
                    TenantName = g.Key.TenantName,
                    TotalLogs = g.Count(),
                    ErrorCount = g.Count(l => l.Level >= SystemLogLevel.Error),
                    WarningCount = g.Count(l => l.Level == SystemLogLevel.Warning),
                    LastErrorTime = g.Where(l => l.Level >= SystemLogLevel.Error)
                                     .Max(l => (DateTime?)l.Timestamp)
                })
                .OrderByDescending(t => t.ErrorCount)
                .ToList();

            return grouped;
        }

        #endregion

        #region Private Helpers

        private static SystemLogDto MapToDto(SystemLog l)
        {
            return new SystemLogDto
            {
                Id = l.Id,
                TenantId = l.TenantId,
                TenantName = l.Tenant?.Name,
                Level = l.Level,
                Source = l.Source,
                Message = l.Message,
                ExceptionType = l.ExceptionType,
                StackTrace = l.StackTrace,
                RequestPath = l.RequestPath,
                HttpMethod = l.HttpMethod,
                UserId = l.UserId,
                ClientIp = l.ClientIp,
                Timestamp = l.Timestamp,
                DurationMs = l.DurationMs
            };
        }

        #endregion
    }

    #region DTOs

    public class SystemLogDto
    {
        public Guid Id { get; set; }
        public Guid? TenantId { get; set; }
        public string? TenantName { get; set; }
        public SystemLogLevel Level { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ExceptionType { get; set; }
        public string? StackTrace { get; set; }
        public string? RequestPath { get; set; }
        public string? HttpMethod { get; set; }
        public string? UserId { get; set; }
        public string? ClientIp { get; set; }
        public DateTime Timestamp { get; set; }
        public long? DurationMs { get; set; }
    }

    public class SystemLogFilterDto
    {
        public Guid? TenantId { get; set; }
        public SystemLogLevel? Level { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Source { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class SystemHealthDto
    {
        public DateTime Timestamp { get; set; }
        public int TotalLogsLast24h { get; set; }
        public int ErrorsLast24h { get; set; }
        public int CriticalsLast24h { get; set; }
        public int WarningsLast24h { get; set; }
        public int ErrorsLastHour { get; set; }
        public IList<ErrorSummary> MostCommonErrors { get; set; } = new List<ErrorSummary>();
        public IList<TenantErrorCount> TenantErrorCounts { get; set; } = new List<TenantErrorCount>();

        public string HealthStatus => ErrorsLastHour switch
        {
            0 => "Healthy",
            < 5 => "Warning",
            _ => "Critical"
        };
    }

    public class ErrorSummary
    {
        public string Error { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TenantErrorCount
    {
        public Guid TenantId { get; set; }
        public int ErrorCount { get; set; }
    }

    public class TenantHealthSummaryDto
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public int TotalLogs { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public DateTime? LastErrorTime { get; set; }
    }

    #endregion
}
