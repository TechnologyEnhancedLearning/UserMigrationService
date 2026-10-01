using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class OrganisationTransformer : IOrganisationTransformer
{
    public TransformedOrganisation Transform(
        ElfhOrganisation source,
        OrganisationTypeMapping? organisationTypeMapping,
        Guid migrationRunId)
    {
        ArgumentNullException.ThrowIfNull(source);

        var createDate =
            TransformationValueHelper.ToUtc(source.Created)
            ?? DateTimeOffset.UtcNow;

        var amendDate =
            TransformationValueHelper.ToUtc(source.Updated);

        return new TransformedOrganisation
        {
            MigrationRunId = migrationRunId,

            ElfhLocationId = source.LocationId,

            OrganisationId = source.LocationId,

            OrganisationName =
                TransformationValueHelper.NormalizeString(
                    source.LocationName),

            ODSCode =
                TransformationValueHelper.NormalizeString(
                    source.LocationCode),

            PostCode =
                TransformationValueHelper.NormalizeString(
                    source.PostCode),

            OrganisationTypeId =
                organisationTypeMapping?.OrganisationTypeId ?? 0,

            RegionId = null,

            ParentId = source.ParentId,

            CreateDate = createDate,

            CreateUserId = null,

            AmendDate = amendDate,

            AmendUserId = source.AmendUserId,

            RemoveDate =
                source.Deleted
                    ? amendDate
                    : null,

            RemoveUserId =
                source.Deleted
                    ? source.AmendUserId
                    : null,

            CreatedUtc = DateTime.UtcNow
        };
    }
}