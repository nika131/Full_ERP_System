using System.Security.Claims;

namespace NexusERP.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetCurrentUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }

        public static string GetCurrentUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            return user.HasClaim(c => c.Type == "Permission" && c.Value == permission);
        }
    }
}
