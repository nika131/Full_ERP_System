using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using NexusERP.Api.Extensions;
using NexusERP.Domain.Constants;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Api.Controllers
{
    public class PosCashierSwitchDto
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Required, RegularExpression("^[0-9]{4}$")]
        public string Pin { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/pos/auth")]
    [Authorize(Policy = Permissions.ViewPos)]
    public class PosIdentityController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public PosIdentityController(ApplicationDbContext db, IConfiguration configuration, IMemoryCache cache)
        {
            _db = db;
            _configuration = configuration;
            _cache = cache;
        }

        [HttpGet("cashiers")]
        public async Task<IActionResult> GetCashiers()
        {
            int companyId = User.GetCurrentCompanyId();
            var candidates = await _db.Users.AsNoTracking().Include(u => u.Role)
                .Where(u => u.CompanyId == companyId && u.IsActive && u.Role.IsActive && u.PosPin != null)
                .ToListAsync();
            var cashiers = candidates.Where(u => u.Role.Permissions.Contains(Permissions.ViewPos))
                .Select(u => new { u.UserId, u.FullName, u.Username }).ToList();
            return Ok(cashiers);
        }

        [HttpPost("switch")]
        public async Task<IActionResult> Switch([FromBody] PosCashierSwitchDto dto)
        {
            // Only an Account-linked, company-selected terminal owner can switch cashier.
            if (User.FindFirst("token_type")?.Value != "company" ||
                !int.TryParse(User.FindFirst("account_id")?.Value, out int accountId) ||
                !Guid.TryParse(User.FindFirst("session_id")?.Value, out Guid sessionId))
                return Unauthorized(new { message = "Sign in and select a company to activate this POS terminal." });

            int companyId = User.GetCurrentCompanyId();
            string attemptKey = $"pos-pin:{companyId}:{dto.UserId}:{accountId}";
            int attempts = _cache.Get<int>(attemptKey);
            if (attempts >= 5)
                return StatusCode(429, new { message = "Too many incorrect PIN attempts. Try again in 15 minutes." });

            var cashier = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u =>
                u.UserId == dto.UserId && u.CompanyId == companyId && u.IsActive &&
                u.Role.IsActive && u.Role.CompanyId == companyId);

            bool validPin = cashier != null && !string.IsNullOrEmpty(cashier.PosPin) &&
                (cashier.PosPin.StartsWith("$2", StringComparison.Ordinal)
                    ? BCrypt.Net.BCrypt.Verify(dto.Pin, cashier.PosPin)
                    : CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(dto.Pin), Encoding.UTF8.GetBytes(cashier.PosPin)));

            if (!validPin || cashier == null || !cashier.Role.Permissions.Contains(Permissions.ViewPos))
            {
                _cache.Set(attemptKey, attempts + 1, TimeSpan.FromMinutes(15));
                return Unauthorized(new { message = "Incorrect PIN or unavailable cashier." });
            }

            _cache.Remove(attemptKey);

            // Transparently upgrade old clear-text PINs when they are successfully used.
            if (!cashier.PosPin!.StartsWith("$2", StringComparison.Ordinal))
            {
                cashier.PosPin = BCrypt.Net.BCrypt.HashPassword(dto.Pin);
                await _db.SaveChangesAsync();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, cashier.UserId.ToString()),
                new Claim("CompanyId", companyId.ToString()),
                new Claim(ClaimTypes.GivenName, cashier.FullName),
                new Claim(ClaimTypes.Name, cashier.Username),
                new Claim(ClaimTypes.Role, cashier.Role.Name),
                new Claim("role_id", cashier.RoleId.ToString()),
                new Claim("account_id", accountId.ToString()), // terminal owner, not cashier
                new Claim("session_id", sessionId.ToString("D")),
                new Claim("token_type", "pos_cashier"),
                new Claim("pin_version", Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(cashier.PosPin!))))
            };
            claims.AddRange(cashier.Role.Permissions.Select(permission => new Claim("Permission", permission)));

            string jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing.");
            var token = new JwtSecurityTokenHandler().CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), SecurityAlgorithms.HmacSha256)
            });

            return Ok(new { message = $"Signed in to POS as {cashier.FullName}.", token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
