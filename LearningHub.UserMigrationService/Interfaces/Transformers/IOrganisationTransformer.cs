using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IOrganisationTransformer
{
    TransformedOrganisation Transform(ElfhOrganisation source,string? organisationType,Guid migrationRunId);
}