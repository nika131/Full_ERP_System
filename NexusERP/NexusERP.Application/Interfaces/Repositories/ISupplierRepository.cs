using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface ISupplierRepository
    {
        Task<PagedResult<Supplier>> GetPaged(int companyId, int pageNumber, int pageSize, string? searchTerm);
        Task<IEnumerable<Supplier>> GetAllActive(int companyId);
        Task Upsert(Supplier supplier, int userId, int companyId);
        Task Delete(int id, int userId, int companyId);
    }
}