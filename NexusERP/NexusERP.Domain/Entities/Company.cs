using NexusERP.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class Company : IAuditTracked
    {
        public int CompanyId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? JoinCodeHash { get; set; }

        public DateTime? JoinCodeCreatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();

        public ICollection<Role> Roles { get; set; } = new List<Role>();

        public ICollection<Store> Stores { get; set; } = new List<Store>();
    }
}
