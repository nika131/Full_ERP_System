using Microsoft.EntityFrameworkCore;
using NexusERP.Application.DTOs;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateUser(User user, int actorUserId, int companyId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var roleExists = await _context.Roles.AnyAsync(r =>
                        r.RoleId == user.RoleId &&
                        r.CompanyId == companyId &&
                        r.IsActive
                    );

                    if (!roleExists)
                        throw new AppException("The selected role does not belong to this company.");

                    var usernameExists = await _context.Users.AnyAsync(u =>
                        u.CompanyId == companyId &&
                        u.Username == user.Username
                    );

                    if (usernameExists)
                        throw new AppException("Username already exists in this company.");

                    user.CompanyId = companyId;
                    user.IsActive = true;

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    var audit = new SystemAuditLog
                    {
                        CompanyId = companyId,
                        UserId = actorUserId,
                        EntityType = "User",
                        EntityId = user.UserId,
                        Action = "Create",
                        ChangesMade = $"Created User '{user.Username}'"
                    };

                    _context.SystemAuditLogs.Add(audit);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task UpdateUser(User user, int actorUserId, int companyId)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u =>
                u.UserId == user.UserId &&
                u.CompanyId == companyId
            );

            if (existingUser == null)
                throw new AppException("Employee not found.");

            var roleExists = await _context.Roles.AnyAsync(r =>
                r.RoleId == user.RoleId &&
                r.CompanyId == companyId &&
                r.IsActive
            );

            if (!roleExists)
                throw new AppException("The selected role does not belong to this company.");

            var usernameExists = await _context.Users.AnyAsync(u =>
                u.CompanyId == companyId &&
                u.Username == user.Username &&
                u.UserId != user.UserId
            );

            if (usernameExists)
                throw new AppException("Username already exists in this company.");

            existingUser.FullName = user.FullName;
            existingUser.Username = user.Username;
            existingUser.RoleId = user.RoleId;
            existingUser.PosPin = user.PosPin;

            var audit = new SystemAuditLog
            {
                CompanyId = companyId,
                UserId = actorUserId,
                EntityType = "User",
                EntityId = user.UserId,
                Action = "Update",
                ChangesMade = $"Updated User '{user.Username}'"
            };

            _context.SystemAuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(int targetUserId, int actorUserId, int companyId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.UserId == targetUserId &&
                u.CompanyId == companyId
            );

            if (user == null)
                throw new AppException("Employee not found.");

            user.IsActive = false;

            var audit = new SystemAuditLog
            {
                CompanyId = companyId,
                UserId = actorUserId,
                EntityType = "User",
                EntityId = user.UserId,
                Action = "Delete",
                ChangesMade = $"Deleted User '{user.Username}'"
            };

            _context.SystemAuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }

        // Temporary legacy username login.
        public async Task<User?> GetUserByUsername(string username)
        {
            var matches = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .Where(u => u.Username == username)
                .Take(2)
                .ToListAsync();

            return matches.Count == 1 ? matches[0] : null;
        }

        public async Task<User?> GetUserByUsername(string username, int companyId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.CompanyId == companyId &&
                    u.Username == username
                );
        }

        public async Task<PagedResult<User>> GetPagedAsync(
            int companyId,
            int pageNumber,
            int pageSize,
            string? searchTerm,
            string roleFilter)
        {
            var baseQuery = _context.Users
                .Include(u => u.Role)
                .Where(u => u.CompanyId == companyId)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                bool isNumeric = int.TryParse(searchTerm, out int searchId);

                baseQuery = baseQuery.Where(u =>
                    u.Username.Contains(searchTerm) ||
                    u.FullName.Contains(searchTerm) ||
                    (isNumeric && u.UserId == searchId)
                );
            }

            if (!string.IsNullOrWhiteSpace(roleFilter) &&
                !roleFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                baseQuery = baseQuery.Where(u =>
                    u.Role != null &&
                    u.Role.Name == roleFilter
                );
            }

            var totalCount = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderBy(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<User>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task AddSalaryRecordAsync(int companyId, int userId, SalaryRecord record)
        {
            var userExists = await _context.Users.AnyAsync(u =>
                u.UserId == userId &&
                u.CompanyId == companyId
            );

            if (!userExists)
                throw new AppException("Active employee not found.");

            record.CompanyId = companyId;
            record.UserId = userId;

            await _context.SalaryRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SalaryRecord>> GetSalaryHistoryAsync(int companyId, int userId)
        {
            return await _context.SalaryRecords
                .Where(s =>
                    s.CompanyId == companyId &&
                    s.UserId == userId
                )
                .OrderByDescending(s => s.EffectiveDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<UserLookupDto>> GetLookupUsersAsync(int companyId)
        {
            return await _context.Users
                .Where(u => u.CompanyId == companyId)
                .AsNoTracking()
                .Select(u => new UserLookupDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Username = u.Username
                })
                .ToListAsync();
        }

        public async Task<bool> VerifyPosPinAsync(int companyId, int userId, string pin)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.UserId == userId &&
                    u.CompanyId == companyId
                );

            return user != null && user.PosPin == pin;
        }

        public async Task<User?> GetUserById(int companyId, int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.UserId == id &&
                    u.CompanyId == companyId
                );
        }
    }
}