using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Api.DTOs;
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
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            var token = _authService.Login(request.Username, request.Password);

            return Ok(new { token = token });
          
        }

        [HttpPost("register")]
        [Authorize(Policy = "RequireManageUsers")]
        public IActionResult Register([FromBody] RegisterRequestDto request)
        {
            _authService.Register(request.FullName, request.Username, request.Password, request.RoleId);

            return Ok(new { message = "User registered successfully." });
        }
    }
}
