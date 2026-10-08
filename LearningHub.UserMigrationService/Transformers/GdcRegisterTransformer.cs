using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class GdcRegisterTransformer : IGdcRegisterTransformer
{
    public TransformedGdcRegister Transform(
        ElfhGdcRegister source,
        Guid migrationRunId)
    {
        return new TransformedGdcRegister
        {
            MigrationRunId = migrationRunId,

            RegistrationNumber =
                source.RegistrationNumber,

            Dentist =
                source.Dentist ?? false,

            Title =
                source.Title,

            Surname =
                source.Surname,

            Forenames =
                source.Forenames,

            Honorifics =
                source.Honorifics,

            HouseName =
                source.HouseName,

            AddressLine1 =
                source.AddressLine1,

            AddressLine2 =
                source.AddressLine2,

            AddressLine3 =
                source.AddressLine3,

            AddressLine4 =
                source.AddressLine4,

            Town =
                source.Town,

            County =
                source.County,

            PostCode =
                source.PostCode,

            Country =
                source.Country,

            RegistrationDate =
                source.RegistrationDate,

            Qualifications =
                source.Qualifications,

            DcpTitles =
                source.DcpTitles,

            Specialties =
                source.Specialties,

            Condition =
                source.Condition,

            Suspension =
                source.Suspension,

            DateProcessed =
                source.DateProcessed,

            Action =
                source.Action
        };
    }

    private static bool ParseDentist(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (bool.TryParse(value, out var boolValue))
            return boolValue;

        if (int.TryParse(value, out var intValue))
            return intValue != 0;

        return false;
    }
}