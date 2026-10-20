using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/account-auth")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public class AccountAuthController : ControllerBase
    {
        private readonly IAccountRegistrationService _service;

        public AccountAuthController(IAccountRegistrationService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterAccountDto dto)
        {
            await _service.RegisterAsync(dto);
            return Ok(new { message = "If this email is eligible for registration, you will receive a verification code." });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyAccountEmailDto dto)
        {
            await _service.VerifyEmailAsync(dto);
            return Ok(new { message = "Email verified successfully." });
        }

        [HttpPost("resend-code")]
        public async Task<IActionResult> ResendCode([FromBody] ResendAccountCodeDto dto)
        {
            await _service.ResendCodeAsync(dto);
            return Ok(new { message = "If the account requires verification, a new code may have been sent." });
        }
    }
}