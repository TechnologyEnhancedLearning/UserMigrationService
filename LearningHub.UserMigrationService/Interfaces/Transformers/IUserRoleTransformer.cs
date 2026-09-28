using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IUserRoleTransformer
{
    TransformedUserRole Transform(
        int legacyUserId,
        int? legacyAdminLocationId,
        int roleId,
        bool isRemoved,
        Guid migrationRunId);
}