using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
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

        public IEnumerable<SystemAuditLog> GetAll()
        {
            var query = from log in _context.SystemAuditLogs.AsNoTracking()
                        join u in _context.Users.AsNoTracking() on log.UserId equals u.UserId into UserJoin
                        from u in UserJoin.DefaultIfEmpty()
                        orderby log.CreatedAt descending
                        select new SystemAuditLog
                        {
                            LogId = log.LogId,
                            UserId = log.UserId,
                            EntityType = log.EntityType,
                            EntityId = log.EntityId,
                            Action = log.Action,
                            ChangesMade = log.ChangesMade,
                            CreatedAt = log.CreatedAt,
                            PerformedBy = u != null ? u.FullName : "Unknown User"
                        };  

            return query.Take(500).ToList();
        }

        public IEnumerable<SystemAuditLog> SearchLogs(string keyword)
        {
            var baseQuery = from log in _context.SystemAuditLogs.AsNoTracking()
                            join u in _context.Users.AsNoTracking() on log.UserId equals u.UserId into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            select new { log, u };

            var fillteredQuery = from item in baseQuery
                                 where item.log.Action.Contains(keyword) ||
                                       item.log.EntityType.Contains(keyword) ||
                                       item.log.ChangesMade.Contains(keyword) ||
                                       (item.u != null && item.u.FullName.Contains(keyword))
                                orderby item.log.CreatedAt descending
                                select new SystemAuditLog
                                {
                                    LogId = item.log.LogId,
                                    UserId = item.log.UserId,
                                    EntityType = item.log.EntityType,
                                    EntityId = item.log.EntityId,
                                    Action = item.log.Action,
                                    ChangesMade = item.log.ChangesMade,
                                    CreatedAt = item.log.CreatedAt,
                                    PerformedBy = item.u != null ? item.u.FullName : "Unknown User"
                                };

            return fillteredQuery.Take(500).ToList();
        }
     }
}
