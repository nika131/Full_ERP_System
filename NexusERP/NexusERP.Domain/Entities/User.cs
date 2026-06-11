using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexusERP.Application.Interfaces;
using NexusERP.Domain.Enums;

namespace NexusERP.Domain.Entities
{
    public class User : IAuditTracked
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        
        public int RoleId { get; set; }
        public Role Role { get; set; }

        public ICollection<UserAbsence> Absences { get; set; } = new List<UserAbsence>();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } 
    }
}
