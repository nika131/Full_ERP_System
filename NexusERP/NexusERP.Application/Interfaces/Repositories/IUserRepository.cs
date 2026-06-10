using NexusERP.Domain.Entities;
using NexusERP.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task CreateUser(User user, int actorUserId);
        Task<PagedResult<User>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, string roleFilter);
        Task DeleteUser(int id, int actorUserId);
        Task UpdateUser(User user, int actorUserId);
    }
}
