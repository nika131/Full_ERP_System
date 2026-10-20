
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Constants;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/account-auth")]
    public class AccountSessionController : ControllerBase
    {
        private readonly IAccountSessionService _service;

        public AccountSessionController(IAccountSessionService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Login([FromBody] AccountLoginDto dto)
        {
            var result = await _service.LoginAsync(dto);
            return Ok(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Refresh([FromBody] RefreshAccountSessionDto dto)
        {
            var result = await _service.RefreshAsync(dto);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = AccountJwtConstants.AuthenticationScheme)]
        public async Task<IActionResult> Logout()
        {
            string? accountIdClaim = User.FindFirst("account_id")?.Value;
            string? sessionIdClaim = User.FindFirst("session_id")?.Value;

            if (!int.TryParse(accountIdClaim, out int accountId) ||
                !Guid.TryParse(sessionIdClaim, out Guid sessionId))
                throw new UnauthorizedAccessException("Invalid account session.");

            await _service.LogoutAsync(accountId, sessionId);

            return Ok(new { message = "Logged out successfully." });
        }
    }
}
