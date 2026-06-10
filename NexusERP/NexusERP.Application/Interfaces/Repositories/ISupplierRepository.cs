using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface ISupplierRepository
    {
        Task<PagedResult<Supplier>> GetPaged(int pageNumber, int pageSize, string? searchTerm);
        Task<IEnumerable<Supplier>> GetAllActive();
        Task Upsert(Supplier supplier, int userId);
        Task Delete(int id, int userId);

    }
}
