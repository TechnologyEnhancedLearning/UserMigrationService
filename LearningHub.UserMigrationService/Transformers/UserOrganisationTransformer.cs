using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class UserOrganisationTransformer
    : IUserOrganisationTransformer
{
    public TransformedUserOrganisation Transform(
        int legacyUserId,
        int legacyOrganisationId,
        int? legacyUserEmploymentId,
        bool isRemoved,
        Guid migrationRunId)
    {
        return new TransformedUserOrganisation
        {
            MigrationRunId = migrationRunId,

            LegacyUserId = legacyUserId,

            LegacyOrganisationId = legacyOrganisationId,

            LegacyUserEmploymentId = legacyUserEmploymentId,

            IsRemoved = isRemoved
        };
    }
}