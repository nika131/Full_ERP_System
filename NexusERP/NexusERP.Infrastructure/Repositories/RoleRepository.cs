using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Role>> GetPaged(int pageNumber, int pageSize, string? searchTerm)
        {
            var baseQuery = _context.Roles
                .Where(r => r.IsActive)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                baseQuery = baseQuery.Where(r => r.Name.Contains(searchTerm));
            }

            var totalCount = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderBy(r => r.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Role>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<Role>> GetAllActive()
        {
            return await _context.Roles
                .AsNoTracking()
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task Upsert(Role role, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                bool isNew = role.RoleId == 0;
                string action = isNew ? "Create" : "Edit";
                string changes = isNew ? $"Created role '{role.Name}'" : $"Updated role '{role.Name}' with {role.Permissions.Count} permissions";

                if (isNew)
                {
                    role.IsActive = true;
                    _context.Roles.Add(role);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var existing = await _context.Roles.FindAsync(role.RoleId);

                    if (existing == null || !existing.IsActive) 
                        throw new Exception("Role not found");

                    existing.Name = role.Name;
                    existing.Permissions = role.Permissions;
                    await _context.SaveChangesAsync();
                }

                var audit = new SystemAuditLog
                {
                    UserId = userId,
                    EntityType = "Role",
                    EntityId = role.RoleId,
                    Action = action,
                    ChangesMade = changes
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

        public async Task Delete(int id, int userId)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null || !role.IsActive)
                throw new AppException("Role not Found");

            bool isRoleInUse = await _context.Users.AnyAsync(u => u.RoleId == id && u.IsActive);
            if (isRoleInUse)
                throw new AppException("Cannot delete thie role. It is currently assigned to active users.");

            role.IsActive = false;

            var audit = new SystemAuditLog
            {
                UserId = userId,
                EntityType = "Role",
                EntityId = role.RoleId,
                Action = "Delete",
                ChangesMade = $"Deleted role '{role.Name}'"
            };

            _context.SystemAuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }
    }
}
