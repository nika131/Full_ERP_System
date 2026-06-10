using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<PagedResult<Category>> GetPagedCategories(int pageNumber, int pageSize, string? searchTerm);
        Task<IEnumerable<Category>> GetAllActive();
        Task Upsert(Category category, int userId);
        Task Delete(int id, int userId);
    }
}
