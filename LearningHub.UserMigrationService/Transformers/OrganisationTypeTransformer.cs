using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class OrganisationTypeTransformer : IOrganisationTypeTransformer
{
    public TransformedOrganisationType Transform(ElfhSupportingLookup source,int? organisationTypeId,string? organisationType,bool isMapped,Guid migrationRunId)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new TransformedOrganisationType
        {
            MigrationRunId = migrationRunId,

            LegacyOrganisationTypeId =
                source.Id,

            LegacyOrganisationType =
                TransformationValueHelper.NormalizeString(
                    source.Name),

            OrganisationTypeId =
                organisationTypeId,

            OrganisationType =
                TransformationValueHelper.NormalizeString(
                    organisationType),

            IsMapped =
                isMapped,

            IsRemoved =
                source.Deleted

        };
    }
}