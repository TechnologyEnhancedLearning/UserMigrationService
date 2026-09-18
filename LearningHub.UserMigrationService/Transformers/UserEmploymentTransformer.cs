using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class UserEmploymentTransformer : IUserEmploymentTransformer
{
    public TransformedUserEmployment Transform(ElfhUserEmployment source,Guid migrationRunId)    {
        ArgumentNullException.ThrowIfNull(source);

        return new TransformedUserEmployment
        {
            MigrationRunId = migrationRunId,

            // Legacy identity
            LegacyUserEmploymentId = source.UserEmploymentId,
            LegacyUserId = source.UserId,

            // Relationships
            LegacyLocationId = source.LocationId,
            LegacyJobRoleId = source.JobRoleId,

            // Employment dates
            StartDateUtc = TransformationValueHelper.ToUtc(
                source.StartDate),

            EndDateUtc = TransformationValueHelper.ToUtc(
                source.EndDate),

            // Audit
            UpdatedUtc = TransformationValueHelper.ToUtc(
                source.AmendDate),

            LegacyAmendUserId = source.AmendUserId,

            // Removal
            IsRemoved = source.Deleted
        };
    }
}