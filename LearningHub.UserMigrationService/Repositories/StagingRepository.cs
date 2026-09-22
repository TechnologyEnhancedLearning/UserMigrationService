using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models.Transformation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LearningHub.UserMigrationService.Repositories;

public class StagingRepository : IStagingRepository
{
    private readonly string _connectionString;

    public StagingRepository(
        IOptions<DatabaseOptions> databaseOptions)
    {
        _connectionString =
            databaseOptions.Value.LearningHubConnectionString;
    }

    public async Task InsertProfessionalBodyAsync(
        TransformedProfessionalBody professionalBody,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO [migrations].[ProfessionalBody]
            (
                MigrationRunId,
                LegacyProfessionalBodyId,
                LegacyProfessionalBody,
                LegacyProfessionalBodyCode,
                UploadPrefix,
                IncludeOnCerts,
                IsRemoved,
                LegacyAmendUserId,
                LegacyAmendDate,
                ProfessionalBodyId,
                ProfessionalBody,
                IsMapped
            )
            VALUES
            (
                @MigrationRunId,
                @LegacyProfessionalBodyId,
                @LegacyProfessionalBody,
                @LegacyProfessionalBodyCode,
                @UploadPrefix,
                @IncludeOnCerts,
                @IsRemoved,
                @LegacyAmendUserId,
                @LegacyAmendDate,
                @ProfessionalBodyId,
                @ProfessionalBody,
                @IsMapped
            );
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        command.Parameters.Add(
            new SqlParameter(
                "@MigrationRunId",
                professionalBody.MigrationRunId));

        command.Parameters.Add(
            new SqlParameter(
                "@LegacyProfessionalBodyId",
                professionalBody.LegacyProfessionalBodyId));

        command.Parameters.Add(
            new SqlParameter(
                "@LegacyProfessionalBody",
                (object?)professionalBody.LegacyProfessionalBody
                    ?? DBNull.Value));

        command.Parameters.Add(
            new SqlParameter(
                "@LegacyProfessionalBodyCode",
                (object?)professionalBody.LegacyProfessionalBodyCode
                    ?? DBNull.Value));

        command.Parameters.Add(
            new SqlParameter(
                "@UploadPrefix",
                (object?)professionalBody.UploadPrefix
                    ?? DBNull.Value));

        command.Parameters.Add(
            new SqlParameter(
                "@IncludeOnCerts",
                professionalBody.IncludeOnCerts));

        command.Parameters.Add(
            new SqlParameter(
                "@IsRemoved",
                professionalBody.IsRemoved));

        command.Parameters.Add(
            new SqlParameter(
                "@LegacyAmendUserId",
                (object?)professionalBody.LegacyAmendUserId
                    ?? DBNull.Value));

        command.Parameters.Add(
            new SqlParameter(
                "@LegacyAmendDate",
                (object?)professionalBody.LegacyAmendDate
                    ?? DBNull.Value));

        command.Parameters.Add(
            new SqlParameter(
                "@ProfessionalBodyId",
                (object?)professionalBody.ProfessionalBodyId
                    ?? DBNull.Value));

        command.Parameters.Add(
            new SqlParameter(
                "@ProfessionalBody",
                (object?)professionalBody.ProfessionalBody
                    ?? DBNull.Value));

        command.Parameters.Add(
            new SqlParameter(
                "@IsMapped",
                professionalBody.IsMapped));

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }

    public async Task InsertUserAsync(
        TransformedUser user,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO [migrations].[User]
            (
                MigrationRunId,
                LegacyUserId,
                FirstName,
                LastName,
                EmailAddress,
                AltEmailAddress,
                UserName,
                RegistrationCode,
                IsActive,
                IsRemoved,
                PasswordHash,
                MustChangeNextLogin,
                PasswordLifeCounter,
                RemoteLoginKey,
                RemoteLoginGuid,
                RemoteLoginStart,
                RestrictToSso,
                CreatedUtc,
                UpdatedUtc,
                LegacyAmendUserId
            )
            VALUES
            (
                @MigrationRunId,
                @LegacyUserId,
                @FirstName,
                @LastName,
                @EmailAddress,
                @AltEmailAddress,
                @UserName,
                @RegistrationCode,
                @IsActive,
                @IsRemoved,
                @PasswordHash,
                @MustChangeNextLogin,
                @PasswordLifeCounter,
                @RemoteLoginKey,
                @RemoteLoginGuid,
                @RemoteLoginStart,
                @RestrictToSso,
                @CreatedUtc,
                @UpdatedUtc,
                @LegacyAmendUserId
            );
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        AddParam(command, "@MigrationRunId", user.MigrationRunId);
        AddParam(command, "@LegacyUserId", user.LegacyUserId);
        AddParam(command, "@FirstName", user.FirstName);
        AddParam(command, "@LastName", user.LastName);
        AddParam(command, "@EmailAddress", user.EmailAddress);
        AddParam(command, "@AltEmailAddress", user.AltEmailAddress);
        AddParam(command, "@UserName", user.UserName);
        AddParam(command, "@RegistrationCode", user.RegistrationCode);
        AddParam(command, "@IsActive", user.IsActive);
        AddParam(command, "@IsRemoved", user.IsRemoved);
        AddParam(command, "@PasswordHash", user.PasswordHash);
        AddParam(command, "@MustChangeNextLogin", user.MustChangeNextLogin);
        AddParam(command, "@PasswordLifeCounter", user.PasswordLifeCounter);
        AddParam(command, "@RemoteLoginKey", user.RemoteLoginKey);
        AddParam(command, "@RemoteLoginGuid", user.RemoteLoginGuid);
        AddParam(command, "@RemoteLoginStart", user.RemoteLoginStart);
        AddParam(command, "@RestrictToSso", user.RestrictToSso);
        AddParam(command, "@CreatedUtc", user.CreatedUtc);
        AddParam(command, "@UpdatedUtc", user.UpdatedUtc);
        AddParam(command, "@LegacyAmendUserId", user.LegacyAmendUserId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertUserEmploymentAsync(
        TransformedUserEmployment employment,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO [migrations].[UserEmployment]
            (
                MigrationRunId,
                LegacyUserEmploymentId,
                LegacyUserId,
                LegacyLocationId,
                LegacyJobRoleId,
                StartDateUtc,
                EndDateUtc,
                UpdatedUtc,
                LegacyAmendUserId,
                IsRemoved
            )
            VALUES
            (
                @MigrationRunId,
                @LegacyUserEmploymentId,
                @LegacyUserId,
                @LegacyLocationId,
                @LegacyJobRoleId,
                @StartDateUtc,
                @EndDateUtc,
                @UpdatedUtc,
                @LegacyAmendUserId,
                @IsRemoved
            );
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        AddParam(command, "@MigrationRunId", employment.MigrationRunId);
        AddParam(command, "@LegacyUserEmploymentId", employment.LegacyUserEmploymentId);
        AddParam(command, "@LegacyUserId", employment.LegacyUserId);
        AddParam(command, "@LegacyLocationId", employment.LegacyLocationId);
        AddParam(command, "@LegacyJobRoleId", employment.LegacyJobRoleId);
        AddParam(command, "@StartDateUtc", employment.StartDateUtc);
        AddParam(command, "@EndDateUtc", employment.EndDateUtc);
        AddParam(command, "@UpdatedUtc", employment.UpdatedUtc);
        AddParam(command, "@LegacyAmendUserId", employment.LegacyAmendUserId);
        AddParam(command, "@IsRemoved", employment.IsRemoved);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertUserAdminLocationAsync(
        TransformedUserAdminLocation adminLocation,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO [migrations].[UserAdminLocation]
            (
                MigrationRunId,
                LegacyUserId,
                LegacyAdminLocationId,
                IsRemoved
            )
            VALUES
            (
                @MigrationRunId,
                @LegacyUserId,
                @LegacyAdminLocationId,
                @IsRemoved
            );
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        AddParam(command, "@MigrationRunId", adminLocation.MigrationRunId);
        AddParam(command, "@LegacyUserId", adminLocation.LegacyUserId);
        AddParam(command, "@LegacyAdminLocationId", adminLocation.LegacyAdminLocationId);
        AddParam(command, "@IsRemoved", adminLocation.IsRemoved);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertUserGroupReporterAsync(
        TransformedUserGroupReporter groupReporter,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO [migrations].[UserGroupReporter]
            (
                MigrationRunId,
                LegacyUserGroupReporterId,
                LegacyUserId,
                LegacyUserGroupId,
                IsRemoved,
                LegacyAmendUserId,
                UpdatedUtc
            )
            VALUES
            (
                @MigrationRunId,
                @LegacyUserGroupReporterId,
                @LegacyUserId,
                @LegacyUserGroupId,
                @IsRemoved,
                @LegacyAmendUserId,
                @UpdatedUtc
            );
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        AddParam(command, "@MigrationRunId", groupReporter.MigrationRunId);
        AddParam(command, "@LegacyUserGroupReporterId", groupReporter.LegacyUserGroupReporterId);
        AddParam(command, "@LegacyUserId", groupReporter.LegacyUserId);
        AddParam(command, "@LegacyUserGroupId", groupReporter.LegacyUserGroupId);
        AddParam(command, "@IsRemoved", groupReporter.IsRemoved);
        AddParam(command, "@LegacyAmendUserId", groupReporter.LegacyAmendUserId);
        AddParam(command, "@UpdatedUtc", groupReporter.UpdatedUtc);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertOrganisationAsync(
        TransformedOrganisation organisation,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO [migrations].[Organisation]
            (
                MigrationRunId,
                LegacyOrganisationId,
                LegacyOrganisationTypeId,
                LegacyParentOrganisationId,
                LegacyOrganisationCode,
                LegacyOrganisationName,
                LegacyPostCode,
                OrganisationTypeId,
                OrganisationType,
                Region,
                CreatedUtc,
                UpdatedUtc,
                IsRemoved
            )
            VALUES
            (
                @MigrationRunId,
                @LegacyOrganisationId,
                @LegacyOrganisationTypeId,
                @LegacyParentOrganisationId,
                @LegacyOrganisationCode,
                @LegacyOrganisationName,
                @LegacyPostCode,
                @OrganisationTypeId,
                @OrganisationType,
                @Region,
                @CreatedUtc,
                @UpdatedUtc,
                @IsRemoved
            );
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        AddParam(command, "@MigrationRunId", organisation.MigrationRunId);
        AddParam(command, "@LegacyOrganisationId", organisation.LegacyOrganisationId);
        AddParam(command, "@LegacyOrganisationTypeId", organisation.LegacyOrganisationTypeId);
        AddParam(command, "@LegacyParentOrganisationId", organisation.LegacyParentOrganisationId);
        AddParam(command, "@LegacyOrganisationCode", organisation.LegacyOrganisationCode);
        AddParam(command, "@LegacyOrganisationName", organisation.LegacyOrganisationName);
        AddParam(command, "@LegacyPostCode", organisation.LegacyPostCode);
        AddParam(command, "@OrganisationTypeId", organisation.OrganisationTypeId);
        AddParam(command, "@OrganisationType", organisation.OrganisationType);
        AddParam(command, "@Region", organisation.Region);
        AddParam(command, "@CreatedUtc", organisation.CreatedUtc);
        AddParam(command, "@UpdatedUtc", organisation.UpdatedUtc);
        AddParam(command, "@IsRemoved", organisation.IsRemoved);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task InsertOrganisationTypeAsync(
        TransformedOrganisationType organisationType,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO [migrations].[OrganisationType]
            (
                MigrationRunId,
                LegacyOrganisationTypeId,
                LegacyOrganisationType,
                OrganisationTypeId,
                OrganisationType,
                IsMapped,
                IsRemoved
            )
            VALUES
            (
                @MigrationRunId,
                @LegacyOrganisationTypeId,
                @LegacyOrganisationType,
                @OrganisationTypeId,
                @OrganisationType,
                @IsMapped,
                @IsRemoved
            );
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        AddParam(command, "@MigrationRunId", organisationType.MigrationRunId);
        AddParam(command, "@LegacyOrganisationTypeId", organisationType.LegacyOrganisationTypeId);
        AddParam(command, "@LegacyOrganisationType", organisationType.LegacyOrganisationType);
        AddParam(command, "@OrganisationTypeId", organisationType.OrganisationTypeId);
        AddParam(command, "@OrganisationType", organisationType.OrganisationType);
        AddParam(command, "@IsMapped", organisationType.IsMapped);
        AddParam(command, "@IsRemoved", organisationType.IsRemoved);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddParam(
        SqlCommand command,
        string name,
        object? value)
    {
        command.Parameters.Add(
            new SqlParameter(name, value ?? DBNull.Value));
    }
}