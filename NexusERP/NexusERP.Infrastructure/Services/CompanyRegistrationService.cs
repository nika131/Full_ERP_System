using Microsoft.EntityFrameworkCore;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Constants;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;
using System.Security.Cryptography;
using System.Text;

namespace NexusERP.Infrastructure.Services
{
    public class CompanyRegistrationService : ICompanyRegistrationService
    {
        private readonly ApplicationDbContext _context;

        public CompanyRegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CreateCompanyResultDto> CreateCompanyAsync(int accountId, CreateCompanyRequestDto dto)
        {
            string companyName = dto.CompanyName?.Trim() ?? string.Empty;
            string fullName = dto.FullName?.Trim() ?? string.Empty;
            string username = dto.Username?.Trim() ?? string.Empty;

            if (companyName.Length < 2 || companyName.Length > 150)
                throw new AppException("Company name must contain 2–150 characters.");

            if (fullName.Length < 2 || fullName.Length > 150)
                throw new AppException("Full name must contain 2–150 characters.");

            if (username.Length < 2 || username.Length > 100)
                throw new AppException("Username must contain 2–100 characters.");

            if (!string.IsNullOrWhiteSpace(dto.PosPin) &&
                (dto.PosPin.Length != 4 || !dto.PosPin.All(char.IsAsciiDigit)))
                throw new AppException("POS PIN must contain exactly 4 digits.");

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var account = await _context.Accounts.FirstOrDefaultAsync(a =>
                    a.AccountId == accountId &&
                    a.IsActive &&
                    a.EmailVerified
                );

                if (account == null)
                    throw new AppException("A verified account is required to create a company.");

                string joinCode = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
                string joinCodeHash = Convert.ToHexString(
                    SHA256.HashData(Encoding.UTF8.GetBytes(joinCode))
                );

                var company = new Company
                {
                    Name = companyName,
                    JoinCodeHash = joinCodeHash,
                    JoinCodeCreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                var adminRole = new Role
                {
                    CompanyId = company.CompanyId,
                    Name = "Admin",
                    Permissions = Permissions.GetAllPermissions(),
                    IsSystemRole = true,
                    IsActive = true
                };

                _context.Roles.Add(adminRole);
                _context.Roles.AddRange(RoleTemplates.CreateDefaultRoles(company.CompanyId));
                await _context.SaveChangesAsync();

                // Temporary legacy password, deliberately unrelated to the Account password.
                // Account authentication will replace User.PasswordHash later.
                string unusedLegacyPassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

                var adminUser = new User
                {
                    AccountId = account.AccountId,
                    CompanyId = company.CompanyId,
                    RoleId = adminRole.RoleId,
                    FullName = fullName,
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(unusedLegacyPassword),
                    PosPin = string.IsNullOrWhiteSpace(dto.PosPin) ? null : dto.PosPin,
                    IsActive = true
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                _context.SystemSettings.AddRange(
                    new SystemSetting
                    {
                        CompanyId = company.CompanyId,
                        SettingKey = "GlobalLowStockThreshold",
                        SettingValue = "5",
                        Description = "Default low-stock threshold.",
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedByUserId = adminUser.UserId
                    },
                    new SystemSetting
                    {
                        CompanyId = company.CompanyId,
                        SettingKey = "AllowNegativeInventory",
                        SettingValue = "false",
                        Description = "Allow products to have negative stock.",
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedByUserId = adminUser.UserId
                    },
                    new SystemSetting
                    {
                        CompanyId = company.CompanyId,
                        SettingKey = "DiscountPolicy",
                        SettingValue = "Enabled",
                        Description = "Controls manual POS discounts.",
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedByUserId = adminUser.UserId
                    }
                );

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CreateCompanyResultDto
                {
                    CompanyId = company.CompanyId,
                    UserId = adminUser.UserId,
                    RoleId = adminRole.RoleId,
                    JoinCode = joinCode
                };
            });
        }
    }
}