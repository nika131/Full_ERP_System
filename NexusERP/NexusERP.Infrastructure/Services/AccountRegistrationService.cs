using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;
using System.Security.Cryptography;
using System.Text;

namespace NexusERP.Infrastructure.Services
{
    public class AccountRegistrationService : IAccountRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IVerificationEmailSender _emailSender;
        private readonly byte[] _verificationKey;

        private const int MaxAttempts = 5;
        private const int MaxEmailsPerDay = 5;

        public AccountRegistrationService(ApplicationDbContext context, IVerificationEmailSender emailSender, IConfiguration configuration)
        {
            _context = context;
            _emailSender = emailSender;

            string secret = configuration["Auth:VerificationKey"] ?? throw new InvalidOperationException("Email verification secret is not configured.");
            _verificationKey = Encoding.UTF8.GetBytes(secret);

            if (_verificationKey.Length < 32)
                throw new InvalidOperationException("Email verification secret must contain at least 32 bytes.");
        }

        public async Task RegisterAsync(RegisterAccountDto dto)
        {
            string email = dto.Email.Trim();
            string normalizedEmail = NormalizeEmail(email);

            if (email.Length > 320 || string.IsNullOrWhiteSpace(email))
                throw new AppException("Invalid email address.");

            if (dto.Password.Length < 8 || Encoding.UTF8.GetByteCount(dto.Password) > 72)
                throw new AppException("Password must contain at least 8 characters and no more than 72 UTF-8 bytes.");

            bool exists = await _context.Accounts.AnyAsync(a => a.NormalizedEmail == normalizedEmail);

            if (exists) return;

            var account = new Account
            {
                Email = email,
                NormalizedEmail = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                EmailVerified = false,
                IsActive = true
            };

            string code = IssueVerificationCode(account);

            _context.Accounts.Add(account);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
            {
                return;
            }

            await _emailSender.SendVerificationCodeAsync(email, code);
        }

        public async Task VerifyEmailAsync(VerifyAccountEmailDto dto)
        {
            string normalizedEmail = NormalizeEmail(dto.Email);

            var account = await _context.Accounts.FirstOrDefaultAsync(a =>
                a.NormalizedEmail == normalizedEmail &&
                a.IsActive &&
                !a.EmailVerified
            );

            if (account == null || string.IsNullOrEmpty(account.EmailVerificationCodeHash) ||
                !account.EmailVerificationExpiresAt.HasValue ||
                account.EmailVerificationExpiresAt.Value <= DateTime.UtcNow ||
                account.EmailVerificationAttempts >= MaxAttempts)
                throw new AppException("Invalid or expired verification code.");

            byte[] actualHash = ComputeCodeHash(normalizedEmail, dto.Code);
            byte[] expectedHash = Convert.FromHexString(account.EmailVerificationCodeHash);

            if (!CryptographicOperations.FixedTimeEquals(expectedHash, actualHash))
            {
                account.EmailVerificationAttempts++;

                if (account.EmailVerificationAttempts >= MaxAttempts)
                {
                    account.EmailVerificationCodeHash = null;
                    account.EmailVerificationExpiresAt = null;
                }

                await _context.SaveChangesAsync();
                throw new AppException("Invalid or expired verification code.");
            }

            account.EmailVerified = true;
            account.EmailVerificationCodeHash = null;
            account.EmailVerificationExpiresAt = null;
            account.EmailVerificationAttempts = 0;

            await _context.SaveChangesAsync();
        }

        public async Task ResendCodeAsync(ResendAccountCodeDto dto)
        {
            string normalizedEmail = NormalizeEmail(dto.Email);

            var account = await _context.Accounts.FirstOrDefaultAsync(a =>
                a.NormalizedEmail == normalizedEmail &&
                a.IsActive &&
                !a.EmailVerified
            );

            if (account == null) return;

            var now = DateTime.UtcNow;

            if (account.EmailVerificationSentAt.HasValue &&
                now - account.EmailVerificationSentAt.Value < TimeSpan.FromMinutes(1))
                return;

            if (account.EmailVerificationWindowStart.HasValue &&
                now - account.EmailVerificationWindowStart.Value < TimeSpan.FromHours(24) &&
                account.EmailVerificationSentCount >= MaxEmailsPerDay)
                return;

            string code = IssueVerificationCode(account);

            await _context.SaveChangesAsync();
            await _emailSender.SendVerificationCodeAsync(account.Email, code);
        }

        private string IssueVerificationCode(Account account)
        {
            var now = DateTime.UtcNow;

            if (!account.EmailVerificationWindowStart.HasValue ||
                now - account.EmailVerificationWindowStart.Value >= TimeSpan.FromHours(24))
            {
                account.EmailVerificationWindowStart = now;
                account.EmailVerificationSentCount = 0;
            }

            string code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");

            account.EmailVerificationCodeHash = Convert.ToHexString(ComputeCodeHash(account.NormalizedEmail, code));
            account.EmailVerificationExpiresAt = now.AddMinutes(10);
            account.EmailVerificationSentAt = now;
            account.EmailVerificationAttempts = 0;
            account.EmailVerificationSentCount++;

            return code;
        }

        private byte[] ComputeCodeHash(string normalizedEmail, string code)
        {
            using var hmac = new HMACSHA256(_verificationKey);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes($"{normalizedEmail}:{code}"));
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToUpperInvariant();
        }
    }
}