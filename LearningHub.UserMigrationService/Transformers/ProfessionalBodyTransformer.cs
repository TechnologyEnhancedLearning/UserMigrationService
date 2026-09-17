using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class ProfessionalBodyTransformer : IProfessionalBodyTransformer
{
    public TransformedProfessionalBody Transform(
        ElfhProfessionalBody source,
        ProfessionalBodyMapping? mapping,
        Guid migrationRunId)
    {
        return new TransformedProfessionalBody
        {
            MigrationRunId = migrationRunId,

            // Legacy values
            LegacyProfessionalBodyId = source.ProfessionalBodyId,
            LegacyProfessionalBody = source.ProfessionalBody,
            LegacyProfessionalBodyCode = source.ProfessionalBodyCode,
            UploadPrefix = source.UploadPrefix,

            IncludeOnCerts = source.IncludeOnCerts,

            LegacyAmendUserId = source.AmendUserId,
            LegacyAmendDate = ToUtc(source.AmendDate),

            // Transformation
            IsRemoved = source.Deleted,

            // Learning Hub mapping
            ProfessionalBodyId = mapping?.ProfessionalBodyId,
            ProfessionalBody = mapping?.ProfessionalBody,
            IsMapped = mapping?.IsMapped ?? false
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