using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IOrganisationTypeTransformer
{
    TransformedOrganisationType Transform(ElfhSupportingLookup source, int? organisationTypeId,string? organisationType,bool isMapped,Guid migrationRunId);
}