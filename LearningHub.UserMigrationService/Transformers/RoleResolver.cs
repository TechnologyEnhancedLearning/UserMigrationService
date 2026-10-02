using LearningHub.UserMigrationService.Models;

namespace LearningHub.UserMigrationService.Transformers;

public static class RoleResolver
{
    private const string UserAdminLocationRoleName =
        "Local Admin";

    private const string UserGroupReporterRoleName =
        "Reporter";

    public static int ResolveUserAdminLocationRole(
        IReadOnlyCollection<RoleReference> roles)
    {
        return Resolve(
            roles,
            UserAdminLocationRoleName);
    }

    public static int ResolveUserGroupReporterRole(
        IReadOnlyCollection<RoleReference> roles)
    {
        return Resolve(
            roles,
            UserGroupReporterRoleName);
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
            var availableRoles =
                string.Join(
                    ", ",
                    roles
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.Name))
                        .Select(x =>
                            $"{x.Id}:{x.Name!.Trim()}")
                        .OrderBy(x => x));

            throw new InvalidOperationException(
                $"Learning Hub role '{roleName}' was not found. " +
                $"Available roles: {availableRoles}");
        }

        if (matches.Count > 1)
        {
            throw new InvalidOperationException(
                $"Multiple Learning Hub roles named '{roleName}' were found. " +
                $"Role IDs: {string.Join(
                    ", ",
                    matches.Select(x => x.Id))}");
        }

        return matches[0].Id;
    }
}