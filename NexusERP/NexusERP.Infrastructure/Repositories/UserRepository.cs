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
        public void CreateUser(User user)
        {
            var args = new Dictionary<string, object>
            {
                { "@Username", user.Username },
                { "@FullName", user.FullName },
                { "@PasswordHash", user.PasswordHashed },
                { "@Role", user.Role.ToString() }
            };
            DatabaseHelper.ExecuteNonQuery("sp_CreateUser", args);
        }

        public User? GetUserByUsername(string username)
        {
            var args = new Dictionary<string, object> { { "@Username", username } };
            DataTable dt = DatabaseHelper.ExecuteStoredProcedure("sp_GetUserByUsername", args);

            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new User
            {
                UserId = Convert.ToInt32(row["UserId"]),
                FullName = row["FullName"].ToString()!,
                PasswordHashed = row["PasswordHash"].ToString()!,
                Role = Enum.Parse<UserRole>(row["Role"].ToString()!),
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }
    }
}
