using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IOrganisationTransformer
{
    TransformedOrganisation Transform(ElfhOrganisation source,OrganisationTypeMapping? organisationTypeMapping,Guid migrationRunId);
}