using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NexusERP.Application.DTOs;
using NexusERP.Api.Extensions;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Constants;
using NexusERP.Domain.Enums;

namespace NexusERP.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IUserRepository userRepository, IConfiguration configuration, IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var token = await _authService.Login(request.Username, request.Password);

            return Ok(new { token = token });
        }

        [HttpPost("register")]
        [Authorize(Policy = "RequireManageUsers")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            int currentUserId = User.GetCurrentUserId();

            await _authService.Register(request.FullName, request.Username, request.Password, request.RoleId, currentUserId);

            return Ok(new { message = "User registered successfully." });
        }
    }
}
