
using NexusERP.Application.Interfaces;

namespace NexusERP.Domain.Entities
{
    public class Account : IAuditTracked
    {
        public int AccountId { get; set; }

        public string Email { get; set; } = string.Empty;
        public string NormalizedEmail { get; set; } = string.Empty;

        public string? PasswordHash { get; set; }
        public string? GoogleSubject { get; set; }

        public bool EmailVerified { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<AuthSession> Sessions { get; set; } = new List<AuthSession>();
    }
}
