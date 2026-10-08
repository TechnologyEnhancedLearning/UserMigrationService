using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class GmcRegisterTransformer : IGmcRegisterTransformer
{
    public TransformedGmcRegister Transform(
        ElfhGmcRegister source,
        Guid migrationRunId)
    {
        if (string.IsNullOrWhiteSpace(
                source.GmcReferenceNumber))
        {
            throw new InvalidOperationException(
                "GMC reference number is required.");
        }

        return new TransformedGmcRegister
        {
            MigrationRunId = migrationRunId,

            GmcReferenceNumber =
                source.GmcReferenceNumber.Trim(),

            Surname = source.Surname,

            GivenName = source.GivenName,

            YearOfQualification =
                source.YearOfQualification,

            GpRegisterDate =
                source.GpRegisterDate,

            RegistrationStatus =
                source.RegistrationStatus,

            OtherNames =
                source.OtherNames,

            DateProcessed =
                source.DateProcessed.HasValue
                    ? new DateTimeOffset(
                        source.DateProcessed.Value,
                        TimeSpan.Zero)
                    : DateTimeOffset.UtcNow,

            Action =
                source.Action
        };
    }
}