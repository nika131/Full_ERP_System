using NexusERP.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class Role : IAuditTracked
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<string> Permissions { get; set; } = new List<string>();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
