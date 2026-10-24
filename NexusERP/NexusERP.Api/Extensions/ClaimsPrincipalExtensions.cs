using System.Security.Claims;

namespace NexusERP.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetCurrentUserId(this ClaimsPrincipal user)
        {
            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(idClaim, out int userId) || userId <= 0)
                throw new UnauthorizedAccessException("Invalid user context.");

            return userId;
        }

        public static int GetCurrentCompanyId(this ClaimsPrincipal user)
        {
            var companyClaim = user.FindFirst("CompanyId")?.Value;

            if (!int.TryParse(companyClaim, out int companyId) || companyId <= 0)
                throw new UnauthorizedAccessException("Invalid company context.");

            return companyId;
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