using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IUserOrganisationTransformer
{
    TransformedUserOrganisation Transform(
        int legacyUserId,
        int legacyOrganisationId,
        int? legacyUserEmploymentId,
        bool isRemoved,
        Guid migrationRunId);
}