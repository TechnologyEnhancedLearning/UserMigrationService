using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class UserTransformer : IUserTransformer
{
    public TransformedUser Transform(
        ElfhUser source,
        Guid migrationRunId)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new TransformedUser
        {
            MigrationRunId = migrationRunId,

            // Legacy identity
            LegacyUserId = source.UserId,

            // User details
            FirstName = Normalize(source.FirstName),
            LastName = Normalize(source.LastName),
            EmailAddress = Normalize(source.EmailAddress),
            AltEmailAddress = Normalize(source.AltEmailAddress),
            UserName = Normalize(source.UserName),
            RegistrationCode = Normalize(source.RegistrationCode),

            // Account state
            IsActive = source.Active,
            IsRemoved = source.Deleted,

            // Authentication
            PasswordHash = source.PasswordHash,
            MustChangeNextLogin = source.MustChangeNextLogin,
            PasswordLifeCounter = source.PasswordLifeCounter,

            RemoteLoginKey = source.RemoteLoginKey,
            RemoteLoginGuid = source.RemoteLoginGuid,
            RemoteLoginStart = TransformationValueHelper.ToUtc(
                source.RemoteLoginStart),

            RestrictToSso = source.RestrictToSSO,

            // Audit
            CreatedUtc = TransformationValueHelper.ToUtc(
                source.CreatedDate),

            UpdatedUtc = TransformationValueHelper.ToUtc(
                source.AmendDate),

            LegacyAmendUserId = source.AmendUserId
        };
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}