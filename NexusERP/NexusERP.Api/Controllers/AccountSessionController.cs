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
        private const string CookieName = "__Secure-tenexerp-refresh";
        private const string CookiePath = "/api/account-auth";

        private readonly IAccountSessionService _service;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] TrustedOrigins =
        {
            "https://www.tenexerp.com",
            "https://tenexerp.com",
            "http://localhost:5173"
        };

        public AccountSessionController(
            IAccountSessionService service,
            IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        private bool HasTrustedOrigin()
        {
            var origin = Request.Headers["Origin"].ToString();

            return TrustedOrigins.Contains(
                origin,
                StringComparer.Ordinal
            );
        }

        private CookieOptions GetCookieOptions(DateTime? expires = null)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = _env.IsDevelopment()
                    ? SameSiteMode.None
                    : SameSiteMode.Lax,
                Path = CookiePath,
                Expires = expires.HasValue
                    ? new DateTimeOffset(
                        DateTime.SpecifyKind(
                            expires.Value,
                            DateTimeKind.Utc))
                    : null
            };
        }

        private void SetRefreshCookie(AccountSessionResponseDto result)
        {
            Response.Cookies.Append(
                CookieName,
                result.RefreshToken,
                GetCookieOptions(result.RefreshTokenExpiresAt)
            );
        }

        private IActionResult SessionResponse(AccountSessionResponseDto result)
        {
            // Never return the refresh token in JSON.
            return Ok(new
            {
                result.Message,
                result.AccessToken,
                result.AccessTokenExpiresAt,
                result.RefreshTokenExpiresAt
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Login([FromBody] AccountLoginDto dto)
        {
            if (!HasTrustedOrigin())
                return StatusCode(403, new { message = "Untrusted origin." });

            var result = await _service.LoginAsync(dto);

            SetRefreshCookie(result);

            return SessionResponse(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Refresh()
        {
            if (!HasTrustedOrigin())
                return StatusCode(403, new { message = "Untrusted origin." });

            if (!Request.Cookies.TryGetValue(CookieName, out var token)
                || string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized(new { message = "No active session." });
            }

            var result = await _service.RefreshAsync(
                new RefreshAccountSessionDto
                {
                    RefreshToken = token
                }
            );

            SetRefreshCookie(result);

            return SessionResponse(result);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            if (!HasTrustedOrigin())
                return StatusCode(403, new { message = "Untrusted origin." });

            if (Request.Cookies.TryGetValue(CookieName, out var token) &&
                !string.IsNullOrWhiteSpace(token))
            {
                await _service.LogoutByRefreshTokenAsync(token);
            }

            Response.Cookies.Delete(CookieName, GetCookieOptions());

            return Ok(new { message = "Logged out successfully." });
        }
    }
}