using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
using NexusERP.Application.Interfaces.Repositories;

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
        public async Task<IActionResult> GetSystemLogs(
            [FromQuery] int pageSize = 50,
            [FromQuery] DateTime? lastCreatedAt = null,
            [FromQuery] int? lastLogId = null,
            [FromQuery] string? searchTerm = null)
        {
            if(pageSize > 100) pageSize = 100;

            var logs = await _repository.GetPagedLogsOptimized(pageSize, lastCreatedAt, lastLogId, searchTerm);

            var responseItems = logs.Items.Select(log => new AuditLogResponseDto
            {
                LogId = log.LogId,
                UserId = log.UserId,
                PerformedBy = log.User?.FullName ?? "System/Unknown",
                Action = log.Action,
                EntityType = log.EntityType,
                ChangesMade = log.ChangesMade,
                CreatedAt = log.CreatedAt,
            }).ToList();

            return Ok(new
            {
                items = responseItems,
                nextCreatedAt = logs.NextCreatedAt,
                nextLogId = logs.NextId,
                pageSize = logs.PageSize,
                hasMorePages = logs.HasMorePages,
            });
        }
    }
}
