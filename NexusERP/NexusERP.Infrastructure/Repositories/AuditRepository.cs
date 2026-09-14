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


        public async Task<CursorPagedResult<AuditLogResponseDto>> GetPagedLogsOptimized(
            int pageSize, 
            DateTime? lastCreatedAt,
            int? lastLogId,
            string? searchTerm,
            DateTime? startDate,
            DateTime? endDate)
        {
            var minumumDate = startDate ?? DateTime.UtcNow.AddDays(-30);

            var baseQuery = _context.SystemAuditLogs
                .Include(log => log.User)
                .Where(log => log.CreatedAt >= minumumDate)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                bool isNumeric = int.TryParse(searchTerm, out int searchUserId);
                var formattedSearch = $"\"{searchTerm}\"";

                baseQuery = baseQuery.Where(log =>
                    EF.Functions.Contains(log.Action, formattedSearch) ||
                    EF.Functions.Contains(log.EntityType, formattedSearch) ||
                    (log.User != null && log.User.Username.Contains(searchTerm)) ||
                    (isNumeric && log.UserId == searchUserId)
                );
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                baseQuery = baseQuery.Where(log => log.CreatedAt <= endOfDay);
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
                .Select(log => new AuditLogResponseDto
                {
                    LogId = log.LogId,
                    UserId = log.UserId,
                    Username = log.User != null ? log.User.Username : "Deleted User",
                    EntityType = log.EntityType,
                    EntityId = log.EntityId,
                    Action = log.Action,
                    ChangesMade = log.ChangesMade,
                    CreatedAt = log.CreatedAt
                })
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

            return new CursorPagedResult<AuditLogResponseDto>
            {
                Items = items,
                NextCreatedAt = nextCreatedAtCursor,
                NextId = nextLogIdCursor,
                PageSize = pageSize
            };
        }
     }
}
