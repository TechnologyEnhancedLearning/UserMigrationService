using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class UserRoleTransformer : IUserRoleTransformer
{
    public TransformedUserRole Transform(
        int legacyUserId,
        int? legacyAdminLocationId,
        int roleId,
        bool isRemoved,
        Guid migrationRunId)
    {
        return new TransformedUserRole
        {
            MigrationRunId = migrationRunId,

            LegacyUserId = legacyUserId,

            LegacyAdminLocationId = legacyAdminLocationId,

            RoleId = roleId,

            IsRemoved = isRemoved
        };
    }
}