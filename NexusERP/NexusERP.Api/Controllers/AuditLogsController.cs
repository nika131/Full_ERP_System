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
        public IActionResult GetSystemLogs([FromBody] string? keyword)
        {
            try
            {
                var logs = string.IsNullOrWhiteSpace(keyword)
                    ? _repository.GetAll()
                    : _repository.SearchLogs(keyword);

                var response = logs.Select(log => new AuditLogResponseDto
                {
                    LogId = log.LogId,
                    UserId = log.UserId,
                    PerformedBy = log.PerformedBy,
                    Action = log.Action,
                    EntityType = log.EntityType,
                    ChangeMade = log.ChangesMade,
                    CreatedAt = log.CreatedAt
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving audit logs: " + ex.Message });
            }
        }
    }
}
