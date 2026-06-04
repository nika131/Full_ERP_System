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
            try
            {
                if(pageSize > 100) pageSize = 100;

                var logs = _repository.GetPagedLogs(pageNumber, pageSize, searchTerm);

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving audit logs: " + ex.Message });
            }
        }
    }
}
