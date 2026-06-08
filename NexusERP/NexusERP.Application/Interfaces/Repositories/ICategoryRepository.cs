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
        PagedResult<Category> GetPagedCategories(int pageNumber, int pageSize, string? searchTerm);
        IEnumerable<Category> GetAllActive();
        void Upsert(Category category);
        public void Delete(int id);
    }
}
