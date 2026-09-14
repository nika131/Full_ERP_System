using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IAuditRepository
    {
        Task<CursorPagedResult<AuditLogResponseDto>> GetPagedLogsOptimized(
                    int pageSize,
                    DateTime? lastCreatedAt,
                    int? lastLogId,
                    string? searchTerm,
                    DateTime? startDate = null,
                    DateTime? endDate = null);
    }
}
