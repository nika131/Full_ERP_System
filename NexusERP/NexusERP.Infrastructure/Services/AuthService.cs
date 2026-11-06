
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NexusERP.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _jwtSecret;

        public AuthService(IUserRepository userRepository, IConfiguration config)
        {
            _jwtSecret = config["Jwt:Key"] 
                ?? throw new AppException("JWT Secret missing!");
        }

        public string GenerateCompanyJwtToken(User user, int accountId, Guid sessionId, DateTime expiresAt)
        {
            if (user.AccountId != accountId || user.Role == null ||
                !user.Role.IsActive || user.Role.CompanyId != user.CompanyId)
                throw new AppException("Invalid company membership.");

            return CreateUserJwt(user, accountId, sessionId, expiresAt);
        }

        private string CreateUserJwt(User user, int? accountId, Guid? sessionId, DateTime expiresAt)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("CompanyId", user.CompanyId.ToString()),
                new Claim(ClaimTypes.GivenName, user.FullName),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "Unassigned")
            };

            if (accountId.HasValue && sessionId.HasValue)
            {
                claims.Add(new Claim("token_type", "company"));
                claims.Add(new Claim("account_id", accountId.Value.ToString()));
                claims.Add(new Claim("session_id", sessionId.Value.ToString("D")));
                claims.Add(new Claim("role_id", user.RoleId.ToString()));
            }

            if (user.Role != null)
            {
                foreach (var permission in user.Role.Permissions)
                    claims.Add(new Claim("Permission", permission));
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
                    SecurityAlgorithms.HmacSha256
                )
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }

    }
}
