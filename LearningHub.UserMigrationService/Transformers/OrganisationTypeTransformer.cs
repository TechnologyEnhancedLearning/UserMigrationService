using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class OrganisationTypeTransformer : IOrganisationTypeTransformer
{
    public TransformedOrganisationType Transform(ElfhSupportingLookup source,int? organisationTypeId,string? organisationType,bool isMapped,Guid migrationRunId)
    {
        return new TransformedOrganisationType
        {
            MigrationRunId = migrationRunId,

            LegacyOrganisationTypeId = source.Id,
            LegacyOrganisationType = source.Name,

            OrganisationTypeId = organisationTypeId,
            OrganisationType = organisationType,

            IsMapped = isMapped,

            // Supporting lookup currently doesn't expose
            // a deleted flag.
            IsRemoved = false
        };
    }
}