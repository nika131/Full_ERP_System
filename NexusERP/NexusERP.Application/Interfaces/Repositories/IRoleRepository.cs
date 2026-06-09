using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        PagedResult<Role> GetPaged(int pageNumber, int pageSize, string? searchTerm);
        IEnumerable<Role> GetAllActive();
        void Upsert(Role role, int userId);
        void Delete(int id, int userId);
    }
}
