using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Models
{
    public class AuditLogResponseDto
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ChangesMade { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
