
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
        private readonly IUserRepository _userRepository;
        private readonly string _jwtSecret;

        public AuthService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _jwtSecret = config["Jwt:Key"] ?? throw new AppException("JWT Secret missing!");
        }

        public async Task Register(string fullname, string username, string plaintextPassword, int roleId, int actorUserId, int companyId, string posPin)
        {
            var existingUser = await _userRepository.GetUserByUsername(username, companyId);

            if (existingUser != null)
                throw new AppException("Username already exists in this company.");

            var newUser = new User
            {
                CompanyId = companyId,
                FullName = fullname,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(plaintextPassword),
                RoleId = roleId,
                PosPin = posPin
            };

            await _userRepository.CreateUser(newUser, actorUserId, companyId);
        }

        public async Task<string> Login(string username, string plaintextPassword)
        {
            var user = await _userRepository.GetUserByUsername(username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(plaintextPassword, user.PasswordHash))
                throw new AppException("Invalid username or password.");

            return GenerateJwtToken(user);
        }

        public string GenerateJwtToken(User user)
        {
            return CreateUserJwt(user, null, null, DateTime.UtcNow.AddHours(10));
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

        public async Task<string> LoginWithPin(int companyId, int targetUserId, string pin)
        {
            var user = await _userRepository.GetUserById(companyId, targetUserId);

            if (user == null)
                throw new AppException("User not found.");

            if (string.IsNullOrWhiteSpace(user.PosPin) || user.PosPin != pin)
                throw new AppException("Invalid PIN.");

            return GenerateJwtToken(user);
        }
    }
}
