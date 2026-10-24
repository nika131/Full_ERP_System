using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<PagedResult<Category>> GetPagedCategories(int companyId, int pageNumber, int pageSize, string? searchTerm);
        Task<IEnumerable<Category>> GetAllActive(int companyId);
        Task Upsert(Category category, int userId, int companyId);
        Task Delete(int id, int userId, int companyId);
    }
}