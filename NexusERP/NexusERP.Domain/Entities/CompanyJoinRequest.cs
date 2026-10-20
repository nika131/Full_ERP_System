
using NexusERP.Domain.Enums;

namespace NexusERP.Domain.Entities
{
    public class CompanyJoinRequest
    {
        public int JoinRequestId { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;

        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        public CompanyJoinRequestStatus Status { get; set; } = CompanyJoinRequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        public int? ReviewedByUserId { get; set; }
        public User? ReviewedByUser { get; set; }

        public int? ApprovedRoleId { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
