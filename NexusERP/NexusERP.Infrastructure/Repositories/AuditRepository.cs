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

        public async Task<PagedResult<SystemAuditLog>> GetPagedLogs(int pageNumber, int pageSize, string? searchTerm)
        {
            var baseQuery = _context.SystemAuditLogs
                .Include(log => log.User)
                .AsNoTracking();

            var endDate = DateTime.UtcNow.AddDays(-30);

            baseQuery = baseQuery.Where(log => log.CreatedAt >= endDate);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                bool isNumeric = int.TryParse(searchTerm, out int searchUserId);
                baseQuery = baseQuery.Where(log =>
                    log.Action.Contains(searchTerm) ||
                    log.EntityType.Contains(searchTerm) ||
                    log.ChangesMade.Contains(searchTerm) ||
                    (isNumeric && log.UserId == searchUserId)
                );
            }

            var totalCount = await baseQuery.CountAsync();

            var items = await baseQuery
                .OrderByDescending(log => log.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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
