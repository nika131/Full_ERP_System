using NexusERP.Application.DTOs;
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
        Task CreateUser(User user, int actorUserId, int companyId);
        Task<PagedResult<User>> GetPagedAsync(int companyId, int pageNumber, int pageSize, string? searchTerm, string roleFilter);
        Task DeleteUser(int id, int actorUserId, int companyId);
        Task UpdateUser(User user, int actorUserId, int companyId);
        Task<User?> GetUserByUsername(string username);
        Task<User?> GetUserByUsername(string username, int companyId);

        Task AddSalaryRecordAsync(int companyId, int userId, SalaryRecord record);
        Task<IEnumerable<SalaryRecord>> GetSalaryHistoryAsync(int companyId, int userId);
        Task<List<UserLookupDto>> GetLookupUsersAsync(int companyId);
        Task<bool> VerifyPosPinAsync(int companyId, int userId, string pin);
        Task<User?> GetUserById(int companyId, int id);
    }
}
