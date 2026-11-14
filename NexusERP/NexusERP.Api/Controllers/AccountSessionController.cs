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

        private bool IsTrustedRequest(string clientType)
        {
            string origin = Request.Headers.Origin.ToString();

            if (clientType == "Web")
            {
                return TrustedOrigins.Contains(
                    origin,
                    StringComparer.Ordinal
                );
            }

            if (clientType == "POS")
            {
                // Native Android/iOS requests normally have no Origin.
                if (string.IsNullOrWhiteSpace(origin))
                    return true;

                // Allow POS testing through Expo Web locally.
                if (_env.IsDevelopment())
                {
                    return origin == "http://localhost:8081" ||
                           origin == "http://127.0.0.1:8081";
                }

                return false;
            }

            return false;
        }

        private string? GetRefreshToken(RefreshAccountSessionDto dto)
        {
            if (dto.ClientType == "POS")
                return dto.RefreshToken;

            return Request.Cookies.TryGetValue(CookieName, out var token)
                ? token
                : null;
        }

        private IActionResult SessionResponse(
            AccountSessionResponseDto result,
            string clientType)
        {
            if (clientType == "POS")
            {
                return Ok(new
                {
                    result.Message,
                    result.AccessToken,
                    result.RefreshToken,
                    result.AccessTokenExpiresAt,
                    result.RefreshTokenExpiresAt
                });
            }

            SetRefreshCookie(result);

            return Ok(new
            {
                result.Message,
                result.AccessToken,
                result.AccessTokenExpiresAt,
                result.RefreshTokenExpiresAt
            });
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



        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Login(
            [FromBody] AccountLoginDto dto)
        {
            if (!IsTrustedRequest(dto.ClientType))
                return StatusCode(403, new { message = "Untrusted origin." });

            var result = await _service.LoginAsync(dto);

            return SessionResponse(result, dto.ClientType);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshAccountSessionDto dto)
        {
            if (!IsTrustedRequest(dto.ClientType))
                return StatusCode(403, new { message = "Untrusted origin." });

            string? token = GetRefreshToken(dto);

            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized(new { message = "No active session." });

            var result = await _service.RefreshAsync(
                new RefreshAccountSessionDto
                {
                    RefreshToken = token,
                    ClientType = dto.ClientType
                }
            );

            return SessionResponse(result, dto.ClientType);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(
            [FromBody] RefreshAccountSessionDto dto)
        {
            if (!IsTrustedRequest(dto.ClientType))
                return StatusCode(403, new { message = "Untrusted origin." });

            string? token = GetRefreshToken(dto);

            if (!string.IsNullOrWhiteSpace(token))
            {
                await _service.LogoutByRefreshTokenAsync(
                    token,
                    dto.ClientType
                );
            }

            if (dto.ClientType == "Web")
                Response.Cookies.Delete(CookieName, GetCookieOptions());

            return Ok(new { message = "Logged out successfully." });
        }

    }
}