using NexusERP.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Exceptions;
using NexusERP.Domain.Models;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Role>> GetPaged(int companyId, int pageNumber, int pageSize, string? searchTerm)
        {
            var baseQuery = _context.Roles
                .Where(r => r.CompanyId == companyId && r.IsActive)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                baseQuery = baseQuery.Where(r => r.Name.Contains(searchTerm));

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

        public async Task<IEnumerable<Role>> GetAllActive(int companyId)
        {
            return await _context.Roles
                .AsNoTracking()
                .Where(r => r.CompanyId == companyId && r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task Upsert(Role role, int userId, int companyId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var actor = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u =>
                        u.UserId == userId && u.CompanyId == companyId && u.IsActive && u.Role.IsActive);

                    if (actor == null || !actor.Role.Permissions.Contains(Permissions.ManageRoles))
                        throw new UnauthorizedAccessException("You do not have permission to manage roles.");

                    if (string.IsNullOrWhiteSpace(role.Name) || role.Name.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase))
                        throw new AppException("The protected Admin role cannot be created or modified.");

                    if (role.Permissions == null || role.Permissions.Count == 0 ||
                        role.Permissions.Except(Permissions.GetAllPermissions(), StringComparer.Ordinal).Any())
                        throw new AppException("Select one or more valid permissions.");

                    if (role.Permissions.Except(actor.Role.Permissions, StringComparer.Ordinal).Any())
                        throw new AppException("You cannot grant permissions you do not possess.");

                    bool isNew = role.RoleId == 0;

                    bool nameExists = await _context.Roles.AnyAsync(r =>
                        r.CompanyId == companyId &&
                        r.IsActive &&
                        r.Name == role.Name &&
                        r.RoleId != role.RoleId
                    );

                    if (nameExists)
                        throw new AppException("A role with this name already exists.");

                    if (isNew)
                    {
                        role.CompanyId = companyId;
                        role.IsSystemRole = false;
                        role.IsActive = true;

                        _context.Roles.Add(role);
                    }
                    else
                    {
                        var existing = await _context.Roles.FirstOrDefaultAsync(r =>
                            r.RoleId == role.RoleId &&
                            r.CompanyId == companyId &&
                            r.IsActive
                        );

                        if (existing == null)
                            throw new AppException("Role not found.");

                        if (existing.IsSystemRole)
                            throw new AppException("System roles cannot be edited.");

                        existing.Name = role.Name;
                        existing.Permissions = role.Permissions;
                    }

                    await _context.SaveChangesAsync();

                    var audit = new SystemAuditLog
                    {
                        CompanyId = companyId,
                        UserId = userId,
                        EntityType = "Role",
                        EntityId = role.RoleId,
                        Action = isNew ? "Create" : "Edit",
                        ChangesMade = isNew
                            ? $"Created role '{role.Name}'"
                            : $"Updated role '{role.Name}' with {role.Permissions.Count} permissions"
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

        public async Task Delete(int id, int userId, int companyId)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r =>
                r.RoleId == id &&
                r.CompanyId == companyId &&
                r.IsActive
            );

            if (role == null)
                throw new AppException("Role not found.");

            if (role.IsSystemRole)
                throw new AppException("System roles cannot be deleted.");

            bool isRoleInUse = await _context.Users.AnyAsync(u =>
                u.CompanyId == companyId &&
                u.RoleId == id &&
                u.IsActive
            );

            if (isRoleInUse)
                throw new AppException("Cannot delete this role. It is currently assigned to active users.");

            role.IsActive = false;

            var audit = new SystemAuditLog
            {
                CompanyId = companyId,
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