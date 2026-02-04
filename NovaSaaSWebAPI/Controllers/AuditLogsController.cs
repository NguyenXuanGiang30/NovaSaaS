using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Domain.Interfaces;
using NovaSaaS.WebApi.Authorization;
using NovaSaaS.WebApi.Models;

namespace NovaSaaS.WebApi.Controllers
{
    /// <summary>
    /// AuditLogs Controller - Xem lịch sử thao tác.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Tags("Analytics")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(
            IUnitOfWork unitOfWork,
            ILogger<AuditLogsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách audit logs có phân trang.
        /// </summary>
        [HttpGet]
        [RequirePermission("audit.view")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AuditLogDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] string? entityName = null,
            [FromQuery] string? action = null,
            [FromQuery] string? userId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var logs = await _unitOfWork.AuditLogs.FindAsync(log =>
                (string.IsNullOrEmpty(entityName) || log.EntityName == entityName) &&
                (string.IsNullOrEmpty(action) || log.Action == action) &&
                (string.IsNullOrEmpty(userId) || log.UserId == userId) &&
                (!fromDate.HasValue || log.CreateAt >= fromDate.Value) &&
                (!toDate.HasValue || log.CreateAt <= toDate.Value));

            var total = logs.Count();
            var paged = logs
                .OrderByDescending(l => l.CreateAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new AuditLogDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    Action = l.Action,
                    EntityName = l.EntityName,
                    EntityId = l.EntityId,
                    OldValues = l.OldValues,
                    NewValues = l.NewValues,
                    CreatedAt = l.CreateAt
                })
                .ToList();

            var result = new PagedResult<AuditLogDto>
            {
                Items = paged,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            return Ok(ApiResponse<PagedResult<AuditLogDto>>.SuccessResult(result));
        }

        /// <summary>
        /// Lấy chi tiết một audit log.
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission("audit.view")]
        [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuditLog(Guid id)
        {
            var log = await _unitOfWork.AuditLogs.GetByIdAsync(id);
            if (log == null)
            {
                return NotFound(ApiResponse<object>.FailResult("Không tìm thấy bản ghi."));
            }

            return Ok(ApiResponse<AuditLogDto>.SuccessResult(new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                Action = log.Action,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                CreatedAt = log.CreateAt
            }));
        }

        /// <summary>
        /// Lấy lịch sử thay đổi của một entity cụ thể.
        /// </summary>
        [HttpGet("entity/{entityName}/{entityId}")]
        [RequirePermission("audit.view")]
        [ProducesResponseType(typeof(ApiResponse<List<AuditLogDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEntityHistory(string entityName, string entityId)
        {
            var logs = await _unitOfWork.AuditLogs.FindAsync(l =>
                l.EntityName == entityName && l.EntityId == entityId);

            var result = logs
                .OrderByDescending(l => l.CreateAt)
                .Select(l => new AuditLogDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    Action = l.Action,
                    EntityName = l.EntityName,
                    EntityId = l.EntityId,
                    OldValues = l.OldValues,
                    NewValues = l.NewValues,
                    CreatedAt = l.CreateAt
                })
                .ToList();

            return Ok(ApiResponse<List<AuditLogDto>>.SuccessResult(result));
        }

        /// <summary>
        /// Lấy thống kê audit logs.
        /// </summary>
        [HttpGet("stats")]
        [RequirePermission("audit.view")]
        [ProducesResponseType(typeof(ApiResponse<AuditStatsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditStats([FromQuery] int days = 30)
        {
            var fromDate = DateTime.UtcNow.AddDays(-days);
            var logs = await _unitOfWork.AuditLogs.FindAsync(l => l.CreateAt >= fromDate);

            var byAction = logs.GroupBy(l => l.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Action, x => x.Count);

            var byEntity = logs.GroupBy(l => l.EntityName)
                .Select(g => new { Entity = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Entity, x => x.Count);

            var result = new AuditStatsDto
            {
                TotalLogs = logs.Count(),
                LogsByAction = byAction,
                LogsByEntity = byEntity,
                PeriodDays = days
            };

            return Ok(ApiResponse<AuditStatsDto>.SuccessResult(result));
        }
    }

    #region DTOs

    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AuditStatsDto
    {
        public int TotalLogs { get; set; }
        public Dictionary<string, int> LogsByAction { get; set; } = new();
        public Dictionary<string, int> LogsByEntity { get; set; } = new();
        public int PeriodDays { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    #endregion
}
