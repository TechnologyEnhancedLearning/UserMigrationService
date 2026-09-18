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
        ArgumentNullException.ThrowIfNull(source);

        return new TransformedProfessionalBody
        {
            MigrationRunId = migrationRunId,

            LegacyProfessionalBodyId = source.ProfessionalBodyId,
            LegacyProfessionalBody =TransformationValueHelper.NormalizeString(source.ProfessionalBody),
            LegacyProfessionalBodyCode =TransformationValueHelper.NormalizeString(source.ProfessionalBodyCode),

            UploadPrefix =
                TransformationValueHelper.NormalizeString(
                    source.UploadPrefix),

            IncludeOnCerts =
                source.IncludeOnCerts,

            IsRemoved =
                source.Deleted,

            LegacyAmendUserId =
                source.AmendUserId,

            LegacyAmendDate =
                TransformationValueHelper.ToUtc(
                    source.AmendDate),

            ProfessionalBodyId =
                mapping?.ProfessionalBodyId,

            ProfessionalBody =
                TransformationValueHelper.NormalizeString(
                    mapping?.ProfessionalBody),

            IsMapped =
                mapping?.IsMapped ?? false
        };
    }
}