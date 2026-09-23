using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Models.Extraction;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace LearningHub.UserMigrationService.Extractors;

public class SupportingLookupExtractor : ISupportingLookupExtractor
{
    private readonly string _connectionString;

    public SupportingLookupExtractor(
        IOptions<DatabaseOptions> options)
    {
        _connectionString =
            options.Value.LegacyHubConnectionString;
    }

    public async IAsyncEnumerable<ElfhGdcRegister> ExtractGdcAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                reg_number,
                Dentist,
                Title,
                Surname,
                Forenames,
                honorifics,
                house_name,
                address_line1,
                address_line2,
                address_line3,
                address_line4,
                Town,
                County,
                PostCode,
                Country,
                regdate,
                qualifications,
                dcp_titles,
                specialties,
                condition,
                suspension,
                dateProcessed,
                action
            FROM dbo.gdcRegisterTBL
            ORDER BY
                reg_number;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.CommandTimeout = 0;

        await using var reader =
            await command.ExecuteReaderAsync(
                CommandBehavior.SequentialAccess,
                cancellationToken);

        var registrationNumberOrdinal =
            reader.GetOrdinal("reg_number");

        var dentistOrdinal =
            reader.GetOrdinal("Dentist");

        var titleOrdinal =
            reader.GetOrdinal("Title");

        var surnameOrdinal =
            reader.GetOrdinal("Surname");

        var forenamesOrdinal =
            reader.GetOrdinal("Forenames");

        var honorificsOrdinal =
            reader.GetOrdinal("honorifics");

        var houseNameOrdinal =
            reader.GetOrdinal("house_name");

        var addressLine1Ordinal =
            reader.GetOrdinal("address_line1");

        var addressLine2Ordinal =
            reader.GetOrdinal("address_line2");

        var addressLine3Ordinal =
            reader.GetOrdinal("address_line3");

        var addressLine4Ordinal =
            reader.GetOrdinal("address_line4");

        var townOrdinal =
            reader.GetOrdinal("Town");

        var countyOrdinal =
            reader.GetOrdinal("County");

        var postCodeOrdinal =
            reader.GetOrdinal("PostCode");

        var countryOrdinal =
            reader.GetOrdinal("Country");

        var registrationDateOrdinal =
            reader.GetOrdinal("regdate");

        var qualificationsOrdinal =
            reader.GetOrdinal("qualifications");

        var dcpTitlesOrdinal =
            reader.GetOrdinal("dcp_titles");

        var specialtiesOrdinal =
            reader.GetOrdinal("specialties");

        var conditionOrdinal =
            reader.GetOrdinal("condition");

        var suspensionOrdinal =
            reader.GetOrdinal("suspension");

        var dateProcessedOrdinal =
            reader.GetOrdinal("dateProcessed");

        var actionOrdinal =
            reader.GetOrdinal("action");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ElfhGdcRegister
            {
                RegistrationNumber =
                    reader.IsDBNull(registrationNumberOrdinal)
                        ? null
                        : reader.GetString(registrationNumberOrdinal),

                Dentist =
                    reader.IsDBNull(dentistOrdinal)
                        ? null
                        : reader.GetString(dentistOrdinal),

                Title =
                    reader.IsDBNull(titleOrdinal)
                        ? null
                        : reader.GetString(titleOrdinal),

                Surname =
                    reader.IsDBNull(surnameOrdinal)
                        ? null
                        : reader.GetString(surnameOrdinal),

                Forenames =
                    reader.IsDBNull(forenamesOrdinal)
                        ? null
                        : reader.GetString(forenamesOrdinal),

                Honorifics =
                    reader.IsDBNull(honorificsOrdinal)
                        ? null
                        : reader.GetString(honorificsOrdinal),

                HouseName =
                    reader.IsDBNull(houseNameOrdinal)
                        ? null
                        : reader.GetString(houseNameOrdinal),

                AddressLine1 =
                    reader.IsDBNull(addressLine1Ordinal)
                        ? null
                        : reader.GetString(addressLine1Ordinal),

                AddressLine2 =
                    reader.IsDBNull(addressLine2Ordinal)
                        ? null
                        : reader.GetString(addressLine2Ordinal),

                AddressLine3 =
                    reader.IsDBNull(addressLine3Ordinal)
                        ? null
                        : reader.GetString(addressLine3Ordinal),

                AddressLine4 =
                    reader.IsDBNull(addressLine4Ordinal)
                        ? null
                        : reader.GetString(addressLine4Ordinal),

                Town =
                    reader.IsDBNull(townOrdinal)
                        ? null
                        : reader.GetString(townOrdinal),

                County =
                    reader.IsDBNull(countyOrdinal)
                        ? null
                        : reader.GetString(countyOrdinal),

                PostCode =
                    reader.IsDBNull(postCodeOrdinal)
                        ? null
                        : reader.GetString(postCodeOrdinal),

                Country =
                    reader.IsDBNull(countryOrdinal)
                        ? null
                        : reader.GetString(countryOrdinal),

                RegistrationDate =
                    reader.IsDBNull(registrationDateOrdinal)
                        ? null
                        : reader.GetDateTime(registrationDateOrdinal),

                Qualifications =
                    reader.IsDBNull(qualificationsOrdinal)
                        ? null
                        : reader.GetString(qualificationsOrdinal),

                DcpTitles =
                    reader.IsDBNull(dcpTitlesOrdinal)
                        ? null
                        : reader.GetString(dcpTitlesOrdinal),

                Specialties =
                    reader.IsDBNull(specialtiesOrdinal)
                        ? null
                        : reader.GetString(specialtiesOrdinal),

                Condition =
                    reader.IsDBNull(conditionOrdinal)
                        ? null
                        : reader.GetString(conditionOrdinal),

                Suspension =
                    reader.IsDBNull(suspensionOrdinal)
                        ? null
                        : reader.GetString(suspensionOrdinal),

                DateProcessed =
                    reader.IsDBNull(dateProcessedOrdinal)
                        ? null
                        : reader.GetDateTime(dateProcessedOrdinal),

                Action =
                    reader.IsDBNull(actionOrdinal)
                        ? null
                        : reader.GetString(actionOrdinal)
            };
        }
    }

    public async IAsyncEnumerable<ElfhGmcRegister> ExtractGmcAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                GMC_Ref_No,
                Surname,
                Given_Name,
                Year_Of_Qualification,
                GP_Register_Date,
                Registration_Status,
                Other_Names,
                dateProcessed,
                action
            FROM dbo.gmclrmpTBL
            ORDER BY
                GMC_Ref_No;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.CommandTimeout = 0;

        await using var reader =
            await command.ExecuteReaderAsync(
                CommandBehavior.SequentialAccess,
                cancellationToken);

        var gmcReferenceNumberOrdinal =
            reader.GetOrdinal("GMC_Ref_No");

        var surnameOrdinal =
            reader.GetOrdinal("Surname");

        var givenNameOrdinal =
            reader.GetOrdinal("Given_Name");

        var yearOfQualificationOrdinal =
            reader.GetOrdinal("Year_Of_Qualification");

        var gpRegisterDateOrdinal =
            reader.GetOrdinal("GP_Register_Date");

        var registrationStatusOrdinal =
            reader.GetOrdinal("Registration_Status");

        var otherNamesOrdinal =
            reader.GetOrdinal("Other_Names");

        var dateProcessedOrdinal =
            reader.GetOrdinal("dateProcessed");

        var actionOrdinal =
            reader.GetOrdinal("action");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ElfhGmcRegister
            {
                GmcReferenceNumber =
                    reader.IsDBNull(gmcReferenceNumberOrdinal)
                        ? null
                        : reader.GetString(gmcReferenceNumberOrdinal),

                Surname =
                    reader.IsDBNull(surnameOrdinal)
                        ? null
                        : reader.GetString(surnameOrdinal),

                GivenName =
                    reader.IsDBNull(givenNameOrdinal)
                        ? null
                        : reader.GetString(givenNameOrdinal),

                YearOfQualification =
                    reader.IsDBNull(yearOfQualificationOrdinal)
                        ? null
                        : reader.GetInt32(yearOfQualificationOrdinal),

                GpRegisterDate =
                    reader.IsDBNull(gpRegisterDateOrdinal)
                        ? null
                        : reader.GetDateTime(gpRegisterDateOrdinal),

                RegistrationStatus =
                    reader.IsDBNull(registrationStatusOrdinal)
                        ? null
                        : reader.GetString(registrationStatusOrdinal),

                OtherNames =
                    reader.IsDBNull(otherNamesOrdinal)
                        ? null
                        : reader.GetString(otherNamesOrdinal),

                DateProcessed =
                    reader.IsDBNull(dateProcessedOrdinal)
                        ? null
                        : reader.GetDateTime(dateProcessedOrdinal),

                Action =
                    reader.IsDBNull(actionOrdinal)
                        ? null
                        : reader.GetString(actionOrdinal)
            };
        }
    }
    public async IAsyncEnumerable<ElfhSupportingLookup>
    ExtractOrganisationTypesAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        const string sql = """
        SELECT
            locationTypeID,
            locationType
        FROM dbo.locationTypeTBL
        ORDER BY
            locationTypeID;
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(
            cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        await using var reader =
            await command.ExecuteReaderAsync(
                CommandBehavior.SequentialAccess,
                cancellationToken);

        var idOrdinal =
            reader.GetOrdinal("locationTypeID");

        var nameOrdinal =
            reader.GetOrdinal("locationType");

        while (await reader.ReadAsync(
            cancellationToken))
        {
            yield return new ElfhSupportingLookup
            {
                LookupType = "OrganisationType",

                Id =
                    reader.GetInt32(idOrdinal),

                Name =
                    reader.IsDBNull(nameOrdinal)
                        ? null
                        : reader.GetString(nameOrdinal),

                Description = null,

                // The supplied locationTypeTBL query
                // does not contain a deleted column.
                Deleted = false
            };
        }
    }
}