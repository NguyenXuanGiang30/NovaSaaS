using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Infrastructure.Migrations;

namespace NovaSaaS.WebApi.Controllers.Admin
{
    /// <summary>
    /// Migration Controller - API để MasterAdmin quản lý database migrations.
    /// Chỉ MasterAdmin mới có quyền truy cập.
    /// </summary>
    [ApiController]
    [Route("api/admin/migrations")]
    [Authorize(Policy = "MasterAdminOnly")]
    [Tags("Admin - Migrations")]
    public class MigrationController : ControllerBase
    {
        private readonly SchemaMigrationRunner _migrationRunner;
        private readonly ILogger<MigrationController> _logger;

        public MigrationController(
            SchemaMigrationRunner migrationRunner,
            ILogger<MigrationController> logger)
        {
            _migrationRunner = migrationRunner;
            _logger = logger;
        }

        /// <summary>
        /// Chạy migrations cho tất cả tenant schemas.
        /// </summary>
        /// <param name="request">Cấu hình migration</param>
        /// <returns>Kết quả migration</returns>
        [HttpPost("run")]
        [ProducesResponseType(typeof(MigrationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RunMigrations([FromBody] RunMigrationRequest? request)
        {
            _logger.LogInformation("🚀 MasterAdmin initiated schema migrations");

            var concurrency = request?.Concurrency ?? 10;
            var result = await _migrationRunner.RunMigrationsAsync(concurrency);

            var response = new MigrationResultDto
            {
                Success = result.Success,
                TotalTenants = result.TotalTenants,
                SuccessCount = result.SuccessCount,
                FailedCount = result.FailedCount,
                DurationMs = (int)result.Duration.TotalMilliseconds,
                ErrorMessage = result.ErrorMessage,
                Errors = result.Errors.Select(e => new TenantErrorDto
                {
                    TenantId = e.TenantId,
                    Subdomain = e.Subdomain,
                    SchemaName = e.SchemaName,
                    ErrorMessage = e.ErrorMessage
                }).ToList()
            };

            if (result.Success)
            {
                return Ok(response);
            }

            return StatusCode(500, response);
        }

        /// <summary>
        /// Chạy migration cho một tenant cụ thể.
        /// </summary>
        [HttpPost("run/{tenantId:guid}")]
        [ProducesResponseType(typeof(TenantMigrationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RunSingleTenantMigration(Guid tenantId)
        {
            _logger.LogInformation("🚀 Running migration for tenant: {TenantId}", tenantId);

            var result = await _migrationRunner.MigrateSingleTenantAsync(tenantId);

            var response = new TenantMigrationResultDto
            {
                TenantId = result.TenantId,
                Success = result.Success,
                MigrationsApplied = result.MigrationsApplied,
                ErrorMessage = result.ErrorMessage
            };

            if (result.Success)
            {
                return Ok(response);
            }

            if (result.ErrorMessage == "Tenant not found")
            {
                return NotFound(response);
            }

            return StatusCode(500, response);
        }

        /// <summary>
        /// Kiểm tra pending migrations (dry-run).
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(PendingMigrationsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingMigrations()
        {
            // TODO: Implement pending migrations check
            return Ok(new PendingMigrationsDto
            {
                HasPendingMigrations = false,
                PendingMigrations = new List<string>()
            });
        }
    }

    #region DTOs

    public class RunMigrationRequest
    {
        /// <summary>
        /// Số tenant được migrate song song (mặc định: 10)
        /// </summary>
        public int Concurrency { get; set; } = 10;
    }

    public class MigrationResultDto
    {
        public bool Success { get; set; }
        public int TotalTenants { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int DurationMs { get; set; }
        public string? ErrorMessage { get; set; }
        public List<TenantErrorDto> Errors { get; set; } = new();
    }

    public class TenantMigrationResultDto
    {
        public Guid TenantId { get; set; }
        public bool Success { get; set; }
        public int MigrationsApplied { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class TenantErrorDto
    {
        public Guid TenantId { get; set; }
        public string Subdomain { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    public class PendingMigrationsDto
    {
        public bool HasPendingMigrations { get; set; }
        public List<string> PendingMigrations { get; set; } = new();
    }

    #endregion
}
