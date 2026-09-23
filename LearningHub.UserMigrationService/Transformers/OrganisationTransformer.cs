using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class OrganisationTransformer
    : IOrganisationTransformer
{
    public TransformedOrganisation Transform(
        ElfhOrganisation source,
        OrganisationTypeMapping? organisationTypeMapping,
        Guid migrationRunId)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new TransformedOrganisation
        {
            MigrationRunId = migrationRunId,

            // Legacy identifiers
            LegacyOrganisationId =
                source.LocationId,

            LegacyOrganisationTypeId =
                source.LocationTypeId,

            LegacyParentOrganisationId =
                source.ParentId,

            // Legacy values
            LegacyOrganisationCode =
                TransformationValueHelper.NormalizeString(
                    source.LocationCode),

            LegacyOrganisationName =
                TransformationValueHelper.NormalizeString(
                    source.LocationName),

            LegacyPostCode =
                TransformationValueHelper.NormalizeString(
                    source.PostCode),

            // Organisation Type mapping
            OrganisationTypeId =
                organisationTypeMapping?.OrganisationTypeId,

            OrganisationType =
                TransformationValueHelper.NormalizeString(
                    organisationTypeMapping?.OrganisationType),

            // Derived region
            Region =
                OrganisationRegionResolver.Resolve(
                    source.PostCode),

            // Audit fields
            CreatedUtc =
                TransformationValueHelper.ToUtc(
                    source.Created),

            UpdatedUtc =
                TransformationValueHelper.ToUtc(
                    source.Updated),

            // Removal status
            IsRemoved =
                source.Deleted
        };
    }
}