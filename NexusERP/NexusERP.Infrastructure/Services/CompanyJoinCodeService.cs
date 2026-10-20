
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
    public class CompanyJoinCodeService : ICompanyJoinCodeService
    {
        private readonly ApplicationDbContext _context;

        public CompanyJoinCodeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RotateCompanyJoinCodeResponseDto> RotateAsync(int companyId, int actorUserId)
        {
            var actor = await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.UserId == actorUserId &&
                    u.CompanyId == companyId &&
                    u.IsActive &&
                    u.Company.IsActive &&
                    u.Role.IsActive &&
                    u.Role.CompanyId == companyId
                );

            if (actor == null || !actor.Role.Permissions.Contains(Permissions.ManageJoinCodes))
                throw new UnauthorizedAccessException("You are not authorized to manage company join codes.");

            var company = await _context.Companies.FirstOrDefaultAsync(c =>
                c.CompanyId == companyId &&
                c.IsActive
            );

            if (company == null)
                throw new AppException("Company not found.");

            string newCode = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

            string newCodeHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(newCode))
            );

            DateTime now = DateTime.UtcNow;

            company.JoinCodeHash = newCodeHash;
            company.JoinCodeCreatedAt = now;

            _context.SystemAuditLogs.Add(new SystemAuditLog
            {
                CompanyId = companyId,
                UserId = actorUserId,
                EntityType = "Company",
                EntityId = companyId,
                Action = "RotateJoinCode",
                ChangesMade = "Company join code regenerated."
            });

            await _context.SaveChangesAsync();

            return new RotateCompanyJoinCodeResponseDto
            {
                Message = "Company join code regenerated successfully. The previous code is no longer valid.",
                JoinCode = newCode,
                ExpiresAt = now.Add(CompanyJoinCodePolicy.ValidityPeriod)
            };
        }
    }
}
