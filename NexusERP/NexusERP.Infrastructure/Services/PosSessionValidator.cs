using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NexusERP.Domain.Constants;
using NexusERP.Infrastructure.Database;

namespace NexusERP.Infrastructure.Services
{
    // Called by the ERP JWT bearer handler for every PIN-switched POS request.
    public class PosSessionValidator
    {
        private readonly ApplicationDbContext _db;

        public PosSessionValidator(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ValidateAsync(ClaimsPrincipal principal, string path, string method)
        {
            // PIN cashier tokens are never valid for general ERP or employee-management APIs.
            bool posEndpoint = path.StartsWith("/api/pos/", StringComparison.OrdinalIgnoreCase) &&
                !path.StartsWith("/api/pos/auth/switch", StringComparison.OrdinalIgnoreCase);
            bool storesLookup = string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(path, "/api/stores", StringComparison.OrdinalIgnoreCase);
            if (!posEndpoint && !storesLookup)
                return false;

            if (!int.TryParse(principal.FindFirst("account_id")?.Value, out int ownerAccountId) ||
                !Guid.TryParse(principal.FindFirst("session_id")?.Value, out Guid sessionId) ||
                !int.TryParse(principal.FindFirst("CompanyId")?.Value, out int companyId) ||
                !int.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int cashierId) ||
                !int.TryParse(principal.FindFirst("role_id")?.Value, out int roleId))
                return false;

            bool parentSession = await _db.AuthSessions.AsNoTracking().AnyAsync(s =>
                s.AccountId == ownerAccountId && s.SessionId == sessionId && s.CompanyId == null &&
                s.RevokedAt == null && s.ExpiresAt > DateTime.UtcNow && s.Account.IsActive && s.Account.EmailVerified);
            if (!parentSession)
                return false;

            var owner = await _db.Users.AsNoTracking().Include(u => u.Role).FirstOrDefaultAsync(u =>
                u.AccountId == ownerAccountId && u.CompanyId == companyId && u.IsActive &&
                u.Company.IsActive && u.Role.IsActive && u.Role.CompanyId == companyId);
            bool ownerAllowed = owner != null && owner.Role.Permissions.Contains(Permissions.ViewPos);
            if (!ownerAllowed)
                return false;

            var cashier = await _db.Users.AsNoTracking().Include(u => u.Role).FirstOrDefaultAsync(u =>
                u.UserId == cashierId && u.CompanyId == companyId && u.RoleId == roleId &&
                u.IsActive && u.Company.IsActive && u.Role.IsActive && u.Role.CompanyId == companyId);
            if (cashier == null || !cashier.Role.Permissions.Contains(Permissions.ViewPos))
                return false;

            string expectedPinVersion = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(cashier.PosPin ?? string.Empty)));
            if (principal.FindFirst("pin_version")?.Value != expectedPinVersion)
                return false; // Changing or revoking a PIN invalidates existing cashier tokens.

            var permissionsInToken = principal.FindAll("Permission").Select(c => c.Value).ToHashSet(StringComparer.Ordinal);
            return permissionsInToken.SetEquals(cashier.Role.Permissions) &&
                principal.FindFirst(ClaimTypes.Role)?.Value == cashier.Role.Name;
        }
    }
}
