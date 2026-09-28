using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IUserGroupRoleTransformer
{
    TransformedUserGroupRole Transform(
        int legacyUserId,
        int legacyUserGroupId,
        int? legacyUserGroupReporterId,
        int roleId,
        bool isRemoved,
        Guid migrationRunId);
}