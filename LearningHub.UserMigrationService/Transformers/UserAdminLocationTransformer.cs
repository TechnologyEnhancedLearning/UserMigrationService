using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class UserAdminLocationTransformer : IUserAdminLocationTransformer
{
    public TransformedUserAdminLocation Transform(ElfhUserAdminLocation source,Guid migrationRunId)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new TransformedUserAdminLocation
        {
            MigrationRunId = migrationRunId,

            LegacyUserId = source.UserId,
            LegacyAdminLocationId = source.AdminLocationId,

            IsRemoved = source.Deleted
        };
    }
}