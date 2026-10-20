
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Constants;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;
using System.Security.Cryptography;
using System.Text;

namespace NexusERP.Infrastructure.Services
{
    public class CompanyJoinRequestService : ICompanyJoinRequestService
    {
        private readonly ApplicationDbContext _context;

        public CompanyJoinRequestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> SubmitAsync(int accountId, SubmitCompanyJoinRequestDto dto)
        {
            string code = dto.JoinCode.Trim().ToUpperInvariant();
            string fullName = dto.FullName.Trim();
            string username = dto.Username.Trim();

            if (fullName.Length < 2 || username.Length < 2)
                throw new AppException("Full name and username must contain at least 2 characters.");

            if (fullName.Length > 150 || username.Length > 100)
                throw new AppException("Full name or username is too long.");

            string codeHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(code))
            );

            var account = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a =>
                a.AccountId == accountId && a.IsActive && a.EmailVerified
            );

            if (account == null)
                throw new UnauthorizedAccessException("A verified account is required.");


            DateTime validAfter = DateTime.UtcNow.Subtract(CompanyJoinCodePolicy.ValidityPeriod);

            var company = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c =>
                c.JoinCodeHash == codeHash &&
                c.JoinCodeCreatedAt >= validAfter &&
                c.IsActive
            );

            if (company == null)
                throw new AppException("Invalid company join code.");

            bool membershipExists = await _context.Users.IgnoreQueryFilters().AnyAsync(u =>
                u.AccountId == accountId && u.CompanyId == company.CompanyId
            );

            if (membershipExists)
                throw new AppException("An employee record already exists for your account in this company. Contact the administrator.");

            bool pendingExists = await _context.CompanyJoinRequests.AnyAsync(r =>
                r.AccountId == accountId &&
                r.CompanyId == company.CompanyId &&
                r.Status == CompanyJoinRequestStatus.Pending
            );

            if (pendingExists)
                throw new AppException("You already have a pending join request for this company.");

            var request = new CompanyJoinRequest
            {
                AccountId = accountId,
                CompanyId = company.CompanyId,
                FullName = fullName,
                Username = username,
                Status = CompanyJoinRequestStatus.Pending
            };

            _context.CompanyJoinRequests.Add(request);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
            {
                throw new AppException("A pending join request already exists for this company.");
            }

            return request.JoinRequestId;
        }

        public async Task<List<CompanyJoinRequestDto>> GetMyRequestsAsync(int accountId)
        {
            return await _context.CompanyJoinRequests
                .AsNoTracking()
                .Where(r => r.AccountId == accountId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new CompanyJoinRequestDto
                {
                    JoinRequestId = r.JoinRequestId,
                    CompanyId = r.CompanyId,
                    CompanyName = r.Company.Name,
                    AccountEmail = r.Account.Email,
                    FullName = r.FullName,
                    Username = r.Username,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ReviewedAt = r.ReviewedAt,
                    ApprovedRoleId = r.ApprovedRoleId
                })
                .ToListAsync();
        }

        public async Task<List<CompanyJoinRequestDto>> GetPendingRequestsAsync(int companyId, int reviewerUserId)
        {
            await ValidateReviewerAsync(companyId, reviewerUserId);

            return await _context.CompanyJoinRequests
                .AsNoTracking()
                .Where(r =>
                    r.CompanyId == companyId &&
                    r.Status == CompanyJoinRequestStatus.Pending
                )
                .OrderBy(r => r.CreatedAt)
                .Select(r => new CompanyJoinRequestDto
                {
                    JoinRequestId = r.JoinRequestId,
                    CompanyId = r.CompanyId,
                    CompanyName = r.Company.Name,
                    AccountEmail = r.Account.Email,
                    FullName = r.FullName,
                    Username = r.Username,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ReviewedAt = r.ReviewedAt,
                    ApprovedRoleId = r.ApprovedRoleId
                })
                .ToListAsync();
        }

        public async Task ApproveAsync(int companyId, int reviewerUserId, int requestId, int roleId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();

                await using var transaction = await _context.Database.BeginTransactionAsync();

                var reviewer = await ValidateReviewerAsync(companyId, reviewerUserId);

                var request = await _context.CompanyJoinRequests.FirstOrDefaultAsync(r =>
                    r.JoinRequestId == requestId &&
                    r.CompanyId == companyId &&
                    r.Status == CompanyJoinRequestStatus.Pending
                );

                if (request == null)
                    throw new AppException("Pending join request not found.");

                var account = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a =>
                    a.AccountId == request.AccountId &&
                    a.IsActive &&
                    a.EmailVerified
                );

                if (account == null)
                    throw new AppException("The applicant's account is inactive or unverified.");

                var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r =>
                    r.RoleId == roleId &&
                    r.CompanyId == companyId &&
                    r.IsActive &&
                    !r.IsSystemRole
                );

                if (role == null)
                    throw new AppException("Select an active, non-system role belonging to this company.");

                if (role.Permissions.Except(reviewer.Role.Permissions).Any())
                    throw new AppException("You cannot assign permissions you do not have.");

                bool membershipExists = await _context.Users.IgnoreQueryFilters().AnyAsync(u =>
                    u.AccountId == request.AccountId &&
                    u.CompanyId == companyId
                );

                if (membershipExists)
                    throw new AppException("An employee record already exists for this account in this company.");

                bool usernameExists = await _context.Users.IgnoreQueryFilters().AnyAsync(u =>
                    u.CompanyId == companyId &&
                    u.Username == request.Username
                );

                if (usernameExists)
                    throw new AppException("The requested username is already taken. Reject the request so the applicant can choose another.");

                request.Status = CompanyJoinRequestStatus.Approved;
                request.ReviewedAt = DateTime.UtcNow;
                request.ReviewedByUserId = reviewerUserId;
                request.ApprovedRoleId = roleId;

                // Save the rowversion-protected status change first.
                await _context.SaveChangesAsync();

                string unusedLegacyPassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

                var user = new User
                {
                    AccountId = account.AccountId,
                    CompanyId = companyId,
                    RoleId = roleId,
                    FullName = request.FullName,
                    Username = request.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(unusedLegacyPassword),
                    PosPin = null,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _context.SystemAuditLogs.Add(new SystemAuditLog
                {
                    CompanyId = companyId,
                    UserId = reviewerUserId,
                    EntityType = "CompanyJoinRequest",
                    EntityId = request.JoinRequestId,
                    Action = "Approve",
                    ChangesMade = $"Approved employee '{user.Username}' with role '{role.Name}'"
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            });
        }

        public async Task RejectAsync(int companyId, int reviewerUserId, int requestId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();

                await using var transaction = await _context.Database.BeginTransactionAsync();

                await ValidateReviewerAsync(companyId, reviewerUserId);

                var request = await _context.CompanyJoinRequests.FirstOrDefaultAsync(r =>
                    r.JoinRequestId == requestId &&
                    r.CompanyId == companyId &&
                    r.Status == CompanyJoinRequestStatus.Pending
                );

                if (request == null)
                    throw new AppException("Pending join request not found.");

                request.Status = CompanyJoinRequestStatus.Rejected;
                request.ReviewedAt = DateTime.UtcNow;
                request.ReviewedByUserId = reviewerUserId;
                request.ApprovedRoleId = null;

                _context.SystemAuditLogs.Add(new SystemAuditLog
                {
                    CompanyId = companyId,
                    UserId = reviewerUserId,
                    EntityType = "CompanyJoinRequest",
                    EntityId = request.JoinRequestId,
                    Action = "Reject",
                    ChangesMade = $"Rejected join request from '{request.Username}'"
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            });
        }

        private async Task<User> ValidateReviewerAsync(int companyId, int reviewerUserId)
        {
            var reviewer = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.UserId == reviewerUserId &&
                    u.CompanyId == companyId &&
                    u.IsActive &&
                    u.Company.IsActive &&
                    u.Role.IsActive &&
                    u.Role.CompanyId == companyId
                );

            if (reviewer == null || !reviewer.Role.Permissions.Contains(Permissions.ManageJoinRequests))
                throw new UnauthorizedAccessException("You are not authorized to review join requests.");

            return reviewer;
        }
    }
}
