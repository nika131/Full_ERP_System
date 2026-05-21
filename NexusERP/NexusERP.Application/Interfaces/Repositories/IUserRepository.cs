using NexusERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        void CreateUser(User user);
        User? GetUserByUsername(string username);
        IEnumerable<User> GetAllUsers();
        IEnumerable<User> SearchUsers(string keyword);
        void DeleteUser(int id);
        void UpdateUser(User user);
    }
}
