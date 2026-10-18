using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IAuditRepository
    {
        Task<CursorPagedResult<AuditLogResponseDto>> GetPagedLogsOptimized(int companyId, int pageSize, DateTime? lastCreatedAt, int? lastLogId, string? searchTerm, DateTime? startDate = null, DateTime? endDate = null);
    }
}