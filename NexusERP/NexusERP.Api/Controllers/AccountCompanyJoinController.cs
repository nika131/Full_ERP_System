
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Constants;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/companies")]
    [Authorize(AuthenticationSchemes = AccountJwtConstants.AuthenticationScheme)]
    public class AccountCompanyJoinController : ControllerBase
    {
        private readonly ICompanyJoinRequestService _service;

        public AccountCompanyJoinController(ICompanyJoinRequestService service)
        {
            _service = service;
        }

        [HttpPost("join")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Join([FromBody] SubmitCompanyJoinRequestDto dto)
        {
            if (!int.TryParse(User.FindFirst("account_id")?.Value, out int accountId))
                return Unauthorized(new { message = "Invalid account identity. Please log in again." });

            int requestId = await _service.SubmitAsync(accountId, dto);

            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Join request submitted successfully. Await administrator approval.",
                joinRequestId = requestId
            });
        }

        [HttpGet("join-requests/mine")]
        public async Task<IActionResult> GetMyRequests()
        {
            if (!int.TryParse(User.FindFirst("account_id")?.Value, out int accountId))
                return Unauthorized(new { message = "Invalid account identity. Please log in again." });

            var requests = await _service.GetMyRequestsAsync(accountId);

            return Ok(new
            {
                message = "Your join requests were retrieved successfully.",
                requests
            });
        }
    }
}
