
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Services;
using NexusERP.Domain.Exceptions;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Services
{
    public class CompanyAccessService : ICompanyAccessService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public CompanyAccessService(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<List<MyCompanyDto>> GetMyCompaniesAsync(int accountId)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u =>
                    u.AccountId == accountId &&
                    u.IsActive &&
                    u.Company.IsActive &&
                    u.Role.IsActive &&
                    u.Role.CompanyId == u.CompanyId
                )
                .OrderBy(u => u.Company.Name)
                .Select(u => new MyCompanyDto
                {
                    CompanyId = u.CompanyId,
                    CompanyName = u.Company.Name,
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Username = u.Username,
                    RoleId = u.RoleId,
                    RoleName = u.Role.Name
                })
                .ToListAsync();
        }

        public async Task<SelectCompanyResponseDto> SelectCompanyAsync(int accountId, Guid sessionId, int companyId)
        {
            if (companyId <= 0)
                throw new AppException("Invalid company identifier.");

            bool sessionValid = await _context.AuthSessions.AsNoTracking().AnyAsync(s =>
                s.SessionId == sessionId &&
                s.AccountId == accountId &&
                s.CompanyId == null &&
                s.RevokedAt == null &&
                s.ExpiresAt > DateTime.UtcNow &&
                s.Account.IsActive &&
                s.Account.EmailVerified
            );

            if (!sessionValid)
                throw new UnauthorizedAccessException("Your session has expired. Please log in again.");

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u =>
                    u.AccountId == accountId &&
                    u.CompanyId == companyId &&
                    u.IsActive &&
                    u.Company.IsActive &&
                    u.Role.IsActive &&
                    u.Role.CompanyId == companyId
                );

            if (user == null)
                throw new AppException("You do not have access to this company.");

            DateTime expiresAt = DateTime.UtcNow.AddMinutes(15);

            string token = _authService.GenerateCompanyJwtToken(user, accountId, sessionId, expiresAt);

            return new SelectCompanyResponseDto
            {
                Message = "Company selected successfully.",
                CompanyId = companyId,
                CompanyName = user.Company.Name,
                UserId = user.UserId,
                RoleName = user.Role.Name,
                AccessToken = token,
                AccessTokenExpiresAt = expiresAt
            };
        }
    }
}
