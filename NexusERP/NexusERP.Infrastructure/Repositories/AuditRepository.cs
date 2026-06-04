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
            var baseQuery = from log in _context.SystemAuditLogs.AsNoTracking()
                            join u in _context.Users.AsNoTracking() on log.UserId equals u.UserId into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            select new { log, u };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                baseQuery = baseQuery.Where(item =>
                    item.log.Action.Contains(searchTerm) ||
                    item.log.EntityType.Contains(searchTerm) ||
                    item.log.ChangesMade.Contains(searchTerm) ||
                    (item.u != null && item.u.FullName.Contains(searchTerm)));
            }

            var totalCount = baseQuery.Count();

            var items = baseQuery
                .OrderByDescending(item => item.log.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(item => new SystemAuditLog
                {
                    LogId = item.log.LogId,
                    UserId = item.log.UserId,
                    EntityType = item.log.EntityType,
                    EntityId = item.log.EntityId,
                    Action = item.log.Action,
                    ChangesMade = item.log.ChangesMade,
                    CreatedAt = item.log.CreatedAt,
                    PerformedBy = item.u != null ? item.u.FullName : "Unknown User"
                }).ToList();

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
