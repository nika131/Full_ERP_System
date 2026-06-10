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
        Task<PagedResult<Role>> GetPaged(int pageNumber, int pageSize, string? searchTerm);
        Task<IEnumerable<Role>> GetAllActive();
        Task Upsert(Role role, int userId);
        Task Delete(int id, int userId);
    }
}
