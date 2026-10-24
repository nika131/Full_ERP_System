
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Constants;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NexusERP.Infrastructure.Services
{
    public class AccountSessionService : IAccountSessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly byte[] _jwtKey;

        private static readonly TimeSpan AccessLifetime = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(30);

        public AccountSessionService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;

            string secret = configuration["AccountJwt:Key"] ?? throw new InvalidOperationException("Account JWT key is not configured.");
            _jwtKey = Encoding.UTF8.GetBytes(secret);

            if (_jwtKey.Length < 32)
                throw new InvalidOperationException("Account JWT key must contain at least 32 bytes.");
        }

        public async Task<AccountSessionResponseDto> LoginAsync(AccountLoginDto dto)
        {
            string normalizedEmail = dto.Email.Trim().ToUpperInvariant();

            var account = await _context.Accounts.FirstOrDefaultAsync(a =>
                a.NormalizedEmail == normalizedEmail &&
                a.IsActive
            );

            if (account == null || string.IsNullOrWhiteSpace(account.PasswordHash))
                throw new AppException("Invalid email or password.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, account.PasswordHash))
                throw new AppException("Invalid email or password.");

            if (!account.EmailVerified)
                throw new AppException("Please verify your email before logging in.");

            var now = DateTime.UtcNow;
            string refreshToken = CreateRefreshToken();

            var session = new AuthSession
            {
                AccountId = account.AccountId,
                CompanyId = null,
                RefreshTokenHash = HashRefreshToken(refreshToken),
                ClientType = "Web",
                CreatedAt = now,
                ExpiresAt = now.Add(RefreshLifetime)
            };

            _context.AuthSessions.Add(session);
            await _context.SaveChangesAsync();

            return CreateResponse(account, session, refreshToken, "Logged in successfully.");
        }

        public async Task<AccountSessionResponseDto> RefreshAsync(RefreshAccountSessionDto dto)
        {
            string refreshHash = HashRefreshToken(dto.RefreshToken);

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();

                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                var session = await _context.AuthSessions
                    .Include(s => s.Account)
                    .FirstOrDefaultAsync(s =>
                        s.RefreshTokenHash == refreshHash &&
                        s.CompanyId == null
                    );

                var now = DateTime.UtcNow;

                if (session == null || session.RevokedAt.HasValue || session.ExpiresAt <= now ||
                    !session.Account.IsActive || !session.Account.EmailVerified)
                    throw new AppException("Invalid or expired session. Please log in again.");

                string newRefreshToken = CreateRefreshToken();

                session.RefreshTokenHash = HashRefreshToken(newRefreshToken);
                session.LastUsedAt = now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreateResponse(session.Account, session, newRefreshToken, "Session refreshed successfully.");
            });
        }

        public async Task LogoutAsync(int accountId, Guid sessionId)
        {
            var session = await _context.AuthSessions.FirstOrDefaultAsync(s =>
                s.AccountId == accountId &&
                s.SessionId == sessionId &&
                s.RevokedAt == null
            );

            if (session == null)
                throw new AppException("Session not found or already closed.");

            session.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private AccountSessionResponseDto CreateResponse(Account account, AuthSession session, string refreshToken, string message)
        {
            DateTime accessExpiresAt = DateTime.UtcNow.Add(AccessLifetime);

            var claims = new List<Claim>
            {
                new Claim("account_id", account.AccountId.ToString()),
                new Claim("session_id", session.SessionId.ToString("D")),
                new Claim("token_type", "account")
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = accessExpiresAt,
                Issuer = AccountJwtConstants.Issuer,
                Audience = AccountJwtConstants.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(_jwtKey),
                    SecurityAlgorithms.HmacSha256
                )
            };

            var handler = new JwtSecurityTokenHandler();
            string accessToken = handler.WriteToken(handler.CreateToken(descriptor));

            return new AccountSessionResponseDto
            {
                Message = message,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessExpiresAt,
                RefreshTokenExpiresAt = session.ExpiresAt
            };
        }

        private static string CreateRefreshToken()
        {
            return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(48));
        }

        private static string HashRefreshToken(string refreshToken)
        {
            return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken))
            );
        }
    }
}
