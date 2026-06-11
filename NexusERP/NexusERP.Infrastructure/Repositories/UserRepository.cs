using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Exceptions;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Data.SqlClient.Internal.SqlClientEventSource;

namespace NexusERP.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateUser(User user, int actorUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                user.IsActive = true;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var audit = new SystemAuditLog
                {
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
        }

        public async Task UpdateUser(User user, int actorUserId)
        {
            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser == null) return;

            existingUser.FullName = user.FullName;
            existingUser.Username = user.Username;
            existingUser.RoleId = user.RoleId;

            var audit = new SystemAuditLog
            {
                UserId = actorUserId,
                EntityType = "User",
                EntityId = user.UserId,
                Action = "Update",
                ChangesMade = $"Updated User '{user.Username}'"
            };

            _context.SystemAuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<User>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, string roleFilter)
        {
            var baseQuery = _context.Users
                .Include(u => u.Role)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                baseQuery = baseQuery.Where(u => u.Username.Contains(searchTerm) ||
                                                u.FullName.Contains(searchTerm) ||
                                                u.UserId.ToString().Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(roleFilter) && !roleFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                baseQuery = baseQuery.Where(u => u.Role != null && u.Role.Name == roleFilter);
            }

            var totalCount = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderBy(s => s.CreatedAt)
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

        public async Task DeleteUser(int targetUserId, int actorUserId)
        {
            var user = await _context.Users.FindAsync(targetUserId);

            if (user == null) return;

            user.IsActive = false;


            var audit = new SystemAuditLog
            {
                UserId = actorUserId,
                EntityType = "User",
                EntityId = user.UserId,
                Action = "Delete",
                ChangesMade = $"Deleted User '{user.Username}'"
            };

            _context.SystemAuditLogs.Add(audit);
            await _context.SaveChangesAsync();  
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _context.Users
                .Include(u => u.Role) 
                .AsNoTracking()    
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task AddSalaryRecordAsync(int userId, SalaryRecord record)
        {
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists) throw new AppException("User not found.");

            record.UserId = userId;
            await _context.SalaryRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SalaryRecord>> GetSalaryHistoryAsync(int userId)
        {
            return await _context.SalaryRecords
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.EffectiveDate) 
                .ToListAsync();
        }
    }
}
