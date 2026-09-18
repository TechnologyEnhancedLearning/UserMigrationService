using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class OrganisationTransformer : IOrganisationTransformer
{
    public TransformedOrganisation Transform(ElfhOrganisation source,string? organisationType,Guid migrationRunId)
    {
        return new TransformedOrganisation
        {
            MigrationRunId = migrationRunId,

            // Legacy identifiers
            LegacyOrganisationId = source.LocationId,
            LegacyOrganisationTypeId = source.LocationTypeId,
            LegacyParentOrganisationId = source.ParentId,

            // Legacy/source values
            LegacyOrganisationCode = source.LocationCode,
            LegacyOrganisationName = source.LocationName,
            LegacyPostCode = source.PostCode,

            // Learning Hub values
            OrganisationType = organisationType,

            // Derived value
            Region = OrganisationRegionResolver.Resolve(
                source.PostCode),

            // Audit
            CreatedUtc = ToUtc(source.Created),
            UpdatedUtc = ToUtc(source.Updated),

            // There is currently no Deleted field
            // extracted from locationTBL, so don't invent
            // removal status.
            IsRemoved = source.Deleted
        };
    }

    private static DateTimeOffset? ToUtc(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        return new DateTimeOffset(
            DateTime.SpecifyKind(
                value.Value,
                DateTimeKind.Utc));
    }
}