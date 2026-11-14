
using NexusERP.Domain.Entities;

namespace NexusERP.Domain.Helpers;

public static class DeletedUserDisplay
{
    public static bool IsRecentlyDeleted(User? user, DateTime cutoff)
    {
        return user != null &&
               !user.IsActive &&
               user.DeletedAt.HasValue &&
               user.DeletedAt.Value > cutoff;
    }

    public static string GetName(
        User? user,
        DateTime cutoff,
        bool useUsername = false)
    {
        if (user == null)
            return "Deleted user";

        if (!user.IsActive && !IsRecentlyDeleted(user, cutoff))
            return "Deleted user";

        return useUsername ? user.Username : user.FullName;
    }
}
