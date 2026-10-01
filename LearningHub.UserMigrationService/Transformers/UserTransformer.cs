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

        var createDate =
            TransformationValueHelper.ToUtc(source.CreatedDate)
            ?? DateTimeOffset.UtcNow;

        var amendDate =
            TransformationValueHelper.ToUtc(source.AmendDate);

        return new TransformedUser
        {
            MigrationRunId = migrationRunId,

            ElfhUserId = source.UserId,

            UserId = source.UserId,

            FirstName =
                TransformationValueHelper.NormalizeString(
                    source.FirstName),

            LastName =
                TransformationValueHelper.NormalizeString(
                    source.LastName),

            EmailAddress =
                TransformationValueHelper.NormalizeString(
                    source.EmailAddress),

            RecoveryEmailAddress = null,

            ElfhUserName =
                TransformationValueHelper.NormalizeString(
                    source.UserName) ?? string.Empty,

            ProfessionalBodyId = null,

            ProfessionalRegistrationNumber =TransformationValueHelper.NormalizeString(source.RegistrationCode),

            Active = source.Active,

            PasswordHash = source.PasswordHash,

            MustChangePassword = source.MustChangeNextLogin,

            PasswordLifeCounter = source.PasswordLifeCounter,

            SecurityLifeCounter = null,

            RemoteLoginKey = source.RemoteLoginKey,

            RemoteLoginGuid = source.RemoteLoginGuid,

            RemoteLoginStart =
                TransformationValueHelper.ToUtc(
                    source.RemoteLoginStart),

            RestrictToSSO = source.RestrictToSSO,

            RequestUserLogout = null,

            CreateDate = createDate,

            CreateUserId = null,

            AmendDate = amendDate,

            AmendUserId = source.AmendUserId,

            RemoveDate = source.Deleted
                ? amendDate
                : null,

            RemoveUserId = source.Deleted
                ? source.AmendUserId
                : null,

            RemovalMethodId = null,

            CreatedUtc = DateTimeOffset.UtcNow
        };
    }
}