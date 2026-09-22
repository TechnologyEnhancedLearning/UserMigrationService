using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class OrganisationTransformer : IOrganisationTransformer
{
    public TransformedOrganisation Transform(
        ElfhOrganisation source,
        string? organisationType,
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

            // Learning Hub values
            OrganisationType =
                TransformationValueHelper.NormalizeString(
                    organisationType),

            // Derived value
            Region =
                OrganisationRegionResolver.Resolve(
                    source.PostCode),

            // Audit
            CreatedUtc =
                TransformationValueHelper.ToUtc(
                    source.Created),

            UpdatedUtc =
                TransformationValueHelper.ToUtc(
                    source.Updated),

            // Removal
            IsRemoved =
                source.Deleted
        };
    }
}