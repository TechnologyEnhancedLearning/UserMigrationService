using LearningHub.UserMigrationService.Models;

namespace LearningHub.UserMigrationService.Transformers;

public static class RoleResolver
{
    public static int ResolveUserAdminLocationRole(
        IReadOnlyCollection<RoleReference> roles)
    {
        return Resolve(
            roles,
            "Administrator");
    }

    public static int ResolveUserGroupReporterRole(
        IReadOnlyCollection<RoleReference> roles)
    {
        return Resolve(
            roles,
            "Group Reporter");
    }

    private static int Resolve(
        IReadOnlyCollection<RoleReference> roles,
        string roleName)
    {
        var matches =
            roles
                .Where(x =>
                    string.Equals(
                        x.Name?.Trim(),
                        roleName,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

        if (matches.Count == 0)
        {
            throw new InvalidOperationException(
                $"Learning Hub role '{roleName}' was not found.");
        }

        if (matches.Count > 1)
        {
            throw new InvalidOperationException(
                $"Multiple Learning Hub roles named '{roleName}' were found.");
        }

        return matches[0].Id;
    }
}