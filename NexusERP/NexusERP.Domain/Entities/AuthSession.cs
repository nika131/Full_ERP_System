
namespace NexusERP.Domain.Entities
{
    public class AuthSession
    {
        public Guid SessionId { get; set; } = Guid.NewGuid();

        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;

        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        public string RefreshTokenHash { get; set; } = string.Empty;
        public string ClientType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUsedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
