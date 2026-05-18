using NexusERP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Application.State
{
    public static class UserSession
    {
        public static string JwtToken { get; set; } = string.Empty;
        public static int UserId { get; set; }
        public static string FullName { get; set; } = string.Empty;
        public static string UserName { get; set; } = string.Empty;
        public static UserRole Role { get; set; }

        public static bool IsLoggedIn => !string.IsNullOrEmpty(JwtToken);

        public static void Logout()
        {
            JwtToken = string.Empty;
            UserId = 0;
            FullName = string.Empty;
            UserName = string.Empty;
            Role = UserRole.Cashier;
        }
    }
}
