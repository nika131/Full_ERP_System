using DocumentFormat.OpenXml.Office.CustomUI;
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
     public class AuditRepository : IAuditRepository
     {
        private readonly ApplicationDbContext _context;

        public AuditRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public PagedResult<SystemAuditLog> GetPagedLogs(int pageNumber, int pageSize, string? searchTerm)
        {
            var baseQuery = _context.SystemAuditLogs
                .Include(log => log.User)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                baseQuery = baseQuery.Where(log =>
                    log.Action.Contains(searchTerm) ||
                    log.EntityType.Contains(searchTerm) ||
                    log.ChangesMade.Contains(searchTerm) ||
                    (log.User != null && log.User.FullName.Contains(searchTerm)));
            }

            var totalCount = baseQuery.Count();

            var items = baseQuery
                .OrderByDescending(log => log.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<SystemAuditLog>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
