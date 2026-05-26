using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Domain.Entities
{
    public class SystemAuditLog
    {
        public int LogId { get; set; } 
        public int UserId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId {  get; set; }
        public string Action { get; set; } = string.Empty;
        public string ChangeMade { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        [NotMapped]
        public string PerformedBy { get; set; } = string.Empty;
    }
}
