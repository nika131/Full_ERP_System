using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
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

        public PagedResult<Role> GetPaged(int pageNumber, int pageSize, string? searchTerm)
        {
            var baseQuery = _context.Roles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                baseQuery = baseQuery.Where(r => r.Name.Contains(searchTerm));
            }

            var totalCount = baseQuery.Count();

            var items = baseQuery
                .OrderBy(r => r.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Role>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public IEnumerable<Role> GetAllActive()
        {
            return _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .ToList();
        }

        public void Upsert(Role role, int userId)
        {
            bool isNew = role.RoleId == 0;
            string action = isNew ? "Create" : "Edit";
            string changes = isNew ? $"Created role '{role.Name}'" : $"Updated role '{role.Name}' with {role.Permissions.Count} permissions";

            if (isNew)
            {
                _context.Roles.Add(role);
            }
            else
            {
                var existing = _context.Roles.Find(role.RoleId);
                if (existing == null) throw new Exception("Role not found");

                existing.Name = role.Name;
                existing.Permissions = role.Permissions;
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
            _context.SaveChanges();
        }

        public void Delete(int id, int userId)
        {
            var role = _context.Roles.Find(id);
            if (role != null)
            {
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
                _context.SaveChanges();
            }
        }
    }
}
