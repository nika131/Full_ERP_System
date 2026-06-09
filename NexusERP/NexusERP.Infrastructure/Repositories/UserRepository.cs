using DocumentFormat.OpenXml.Spreadsheet;
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

            var audit = new SystemAuditLog
            {
                UserId = user.UserId,
                EntityType = "User",
                EntityId = user.UserId,
                Action = "Create",
                ChangesMade = $"Created User '{user.Username}'"
            };

            _context.SystemAuditLogs.Add(audit);
            _context.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            var existingUser = _context.Users.Find(user.UserId);
            if (existingUser != null)
            {
                existingUser.FullName = user.FullName;
                existingUser.Username = user.Username;
                existingUser.RoleId = user.RoleId;

                var audit = new SystemAuditLog
                {
                    UserId = user.UserId,
                    EntityType = "User",
                    EntityId = user.UserId,
                    Action = "Update",
                    ChangesMade = $"Updated User '{user.Username}'"
                };

                _context.SystemAuditLogs.Add(audit);
                _context.SaveChanges();
            }
        }
        public IEnumerable<User> SearchUsers(string keyword)
        {
            return _context.Users.AsNoTracking()
                                .Include(u => u.Role)
                                .Where(u =>
                                    (u.Username.Contains(keyword) ||
                                    u.FullName.Contains(keyword) ||
                                    u.UserId.ToString().Contains(keyword)))
                                .ToList();
        }

        public User? GetUserByUsername(string username)
        {
            return _context.Users.AsNoTracking()
                                .Include(u => u.Role)
                                .FirstOrDefault(u => u.Username == username);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users.AsNoTracking()
                                .Include(u => u.Role)
                                .ToList();
        }

        public void DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                user.IsActive = false;

                _context.Users.Add(user);

                var audit = new SystemAuditLog
                {
                    UserId = user.UserId,
                    EntityType = "User",
                    EntityId = user.UserId,
                    Action = "Delete",
                    ChangesMade = $"Deleted User '{user.Username}'"
                };
                _context.SystemAuditLogs.Add(audit);
                _context.SaveChanges();
            }
        }
    }
}
