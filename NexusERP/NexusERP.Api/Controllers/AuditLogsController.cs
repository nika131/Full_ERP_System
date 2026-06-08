using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
using NexusERP.Application.Interfaces.Repositories;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class AuditLogsController : Controller
    {
        private readonly IAuditRepository _repository;

        public AuditLogsController(IAuditRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult GetSystemLogs([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? searchTerm)
        {
            if(pageSize > 100) pageSize = 100;

            var logs = _repository.GetPagedLogs(pageNumber, pageSize, searchTerm);

            var responseItems = logs.Items.Select(log => new AuditLogResponseDto
            {
                LogId = log.LogId,
                UserId = log.UserId,
                PerformedBy = log.User?.FullName ?? "System/Unknown",
                Action = log.Action,
                EntityType = log.EntityType,
                ChangeMade = log.EntityType,
                CreatedAt = log.CreatedAt,
            }).ToList();

            return Ok(new
            {
                items = responseItems,
                totalCount = logs.TotalCount,
                pageNumber = logs.PageNumber,
                pageSize = logs.PageSize,
            });
        }
    }
}
