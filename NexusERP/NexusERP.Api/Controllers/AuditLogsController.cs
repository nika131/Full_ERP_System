using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Infrastructure.Services;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireViewAuditLogs")]
    public class AuditLogsController : Controller
    {
        private readonly IAuditRepository _repository;

        public AuditLogsController(IAuditRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetSystemLogs([FromQuery] int pageSize = 50, [FromQuery] DateTime? lastCreatedAt = null, [FromQuery] int? lastLogId = null, [FromQuery] string? searchTerm = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string timeZoneId = "UTC")
        {
            if (pageSize > 100) pageSize = 100;

            var logs = await _repository.GetPagedLogsOptimized(
                User.GetCurrentCompanyId(),
                pageSize,
                lastCreatedAt,
                lastLogId,
                searchTerm,
                startDate,
                endDate,
                timeZoneId
            );

            return Ok(new
            {
                items = logs.Items,
                nextCreatedAt = logs.NextCreatedAt,
                nextLogId = logs.NextId,
                pageSize = logs.PageSize,
                hasMorePages = logs.HasMorePages
            });
        }
    }
}