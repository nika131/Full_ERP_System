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
        PagedResult<Supplier> GetPaged(int pageNumber, int pageSize, string? searchTerm);
        IEnumerable<Supplier> GetAllActive();
        void Upsert(Supplier supplier, int userId);
        void Delete(int id, int userId);

    }
}
