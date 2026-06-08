using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
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
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            var token = _authService.Login(request.Username, request.Password);

            return Ok(new { token = token });
          
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public IActionResult Register([FromBody] RegisterRequestDto request)
        {
            if (!Enum.TryParse<UserRole>(request.Role, true, out var parseRole))
            {
                return BadRequest(new { message = "Invalid role specified." });
            }

            _authService.Register(request.FullName, request.Username, request.Password, parseRole);

            return Ok(new { message = "User registered successfully." });
        }
    }
}
