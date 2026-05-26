using System.ComponentModel.DataAnnotations.Schema;

namespace NexusERP.Api.DTOs
{
    public class AuditLogResponseDto
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string ChangeMade { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
