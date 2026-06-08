using Microsoft.EntityFrameworkCore;
using NexusERP.Application.Interfaces.Repositories;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void CreateUser(User user)
        {
            user.IsActive = true;
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User? GetUserByUsername(string username)
        {
            return _context.Users.AsNoTracking()
                                .FirstOrDefault(u => u.Username == username);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users.AsNoTracking()
                                .ToList();
        }

        public IEnumerable<User> SearchUsers(string keyword)
        {
            return _context.Users.AsNoTracking()
                                .Where(u => 
                                    (u.Username.Contains(keyword) ||
                                    u.FullName.Contains(keyword) ||
                                    u.UserId.ToString().Contains(keyword)))
                                .ToList();
        }

        public void DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.IsActive = false;
                _context.SaveChanges();
            }
        }

        public void UpdateUser(User user)
        {
            var existingUser = _context.Users.Find(user.UserId);
            if (existingUser != null)
            {
                existingUser.FullName = user.FullName;
                existingUser.Username = user.Username;
                existingUser.Role = user.Role;

                _context.SaveChanges();
            }
        }
    }
}
