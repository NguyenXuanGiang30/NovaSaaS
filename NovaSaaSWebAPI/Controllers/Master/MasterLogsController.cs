using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaSaaS.Application.Services.Master;
using System;
using System.Threading.Tasks;

namespace NovaSaaS.WebApi.Controllers.Master
{
    /// <summary>
    /// MasterLogsController - Centralized Logging cho Master Admin.
    /// </summary>
    [ApiController]
    [Route("api/master/logs")]
    [Authorize(Roles = "MasterAdmin")]
    public class MasterLogsController : ControllerBase
    {
        private readonly SystemLogService _logService;

        public MasterLogsController(SystemLogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Query system logs với phân trang và bộ lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] SystemLogFilterDto filter)
        {
            var logs = await _logService.GetLogsAsync(filter);
            return Ok(logs);
        }

        /// <summary>
        /// Lấy system health summary.
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> GetHealth()
        {
            var health = await _logService.GetSystemHealthAsync();
            return Ok(health);
        }

        /// <summary>
        /// Lấy tenant health summaries.
        /// </summary>
        [HttpGet("tenants-health")]
        public async Task<IActionResult> GetTenantsHealth()
        {
            var health = await _logService.GetTenantsHealthAsync();
            return Ok(health);
        }

        /// <summary>
        /// Lấy errors của một Tenant cụ thể.
        /// </summary>
        [HttpGet("tenants/{tenantId:guid}/errors")]
        public async Task<IActionResult> GetTenantErrors(Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var filter = new SystemLogFilterDto
            {
                TenantId = tenantId,
                Level = NovaSaaS.Domain.Enums.SystemLogLevel.Error,
                Page = page,
                PageSize = pageSize
            };

            var logs = await _logService.GetLogsAsync(filter);
            return Ok(logs);
        }
    }
}
