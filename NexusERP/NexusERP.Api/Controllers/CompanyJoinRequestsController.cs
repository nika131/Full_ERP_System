
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.Extensions;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/company-join-requests")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "RequireManageUsers")]
    public class CompanyJoinRequestsController : ControllerBase
    {
        private readonly ICompanyJoinRequestService _service;

        public CompanyJoinRequestsController(ICompanyJoinRequestService service)
        {
            _service = service;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var requests = await _service.GetPendingRequestsAsync(User.GetCurrentCompanyId(), User.GetCurrentUserId());

            return Ok(new
            {
                message = "Pending join requests retrieved successfully.",
                requests
            });
        }

        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveCompanyJoinRequestDto dto)
        {
            await _service.ApproveAsync(User.GetCurrentCompanyId(), User.GetCurrentUserId(), id, dto.RoleId);

            return Ok(new { message = "Join request approved. Employee access has been created." });
        }

        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            await _service.RejectAsync(User.GetCurrentCompanyId(), User.GetCurrentUserId(), id);

            return Ok(new { message = "Join request rejected successfully." });
        }
    }
}
