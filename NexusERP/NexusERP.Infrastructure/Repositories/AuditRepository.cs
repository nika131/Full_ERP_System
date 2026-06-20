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


        public async Task<CursorPagedResult<SystemAuditLog>> GetPagedLogsOptimized(
            int pageSize, 
            DateTime? lastCreatedAt,
            int? lastLogId,
            string? searchTerm)
        {
            var retentionDate = DateTime.UtcNow.AddDays(-30);

            var baseQuery = _context.SystemAuditLogs
                .Include(log => log.User)
                .Where(log => log.CreatedAt >= retentionDate)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                bool isNumeric = int.TryParse(searchTerm, out int searchUserId);

                var formattedSearch = $"\"{searchTerm}\"";

                baseQuery = baseQuery.Where(log =>
                    EF.Functions.Contains(log.Action, formattedSearch) ||
                    EF.Functions.Contains(log.EntityType, formattedSearch) ||
                    (isNumeric && log.UserId == log.UserId)
                );
            }

            if (lastCreatedAt.HasValue && lastLogId.HasValue)
            {
                baseQuery = baseQuery.Where(log =>
                    log.CreatedAt < lastCreatedAt.Value ||
                    (log.CreatedAt == lastCreatedAt.Value && log.LogId < lastLogId.Value));
            }

            int fetchCount = pageSize + 1;

            var items = await baseQuery 
                .OrderByDescending(log => log.CreatedAt)
                .ThenByDescending(log => log.LogId)
                .Take(fetchCount)
                .ToListAsync();

            bool hasMorePages = items.Count == fetchCount;

            DateTime? nextCreatedAtCursor = null;
            int? nextLogIdCursor = null;

            if (hasMorePages)
            {
                var lastValidItem = items[pageSize - 1];
                nextCreatedAtCursor = lastValidItem.CreatedAt;
                nextLogIdCursor = lastValidItem.LogId;

                items.RemoveAt(items.Count - 1);
            }

            return new CursorPagedResult<SystemAuditLog>
            {
                Items = items,
                NextCreatedAt = nextCreatedAtCursor,
                NextId = nextLogIdCursor,
                PageSize = pageSize
            };
        }
     }
}
