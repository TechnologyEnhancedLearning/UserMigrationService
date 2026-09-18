using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class UserGroupReporterTransformer : IUserGroupReporterTransformer
{
    public TransformedUserGroupReporter Transform(ElfhUserGroupReporter source,Guid migrationRunId)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new TransformedUserGroupReporter
        {
            MigrationRunId = migrationRunId,

            LegacyUserGroupReporterId =
                source.UserGroupReporterId,

            LegacyUserId =
                source.UserId,

            LegacyUserGroupId =
                source.UserGroupId,

            IsRemoved =
                source.Deleted,

            LegacyAmendUserId =
                source.AmendUserId,

            UpdatedUtc =
                TransformationValueHelper.ToUtc(source.AmendDate)
        };
    }
}