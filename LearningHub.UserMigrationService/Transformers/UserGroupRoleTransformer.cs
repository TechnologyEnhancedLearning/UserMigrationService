using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class UserGroupRoleTransformer : IUserGroupRoleTransformer
{
    public TransformedUserGroupRole Transform(
        int legacyUserId,
        int legacyUserGroupId,
        int? legacyUserGroupReporterId,
        int roleId,
        bool isRemoved,
        Guid migrationRunId)
    {
        return new TransformedUserGroupRole
        {
            MigrationRunId = migrationRunId,

            LegacyUserId = legacyUserId,

            LegacyUserGroupId = legacyUserGroupId,

            LegacyUserGroupReporterId =
                legacyUserGroupReporterId,

            RoleId = roleId,

            IsRemoved = isRemoved
        };
    }
}