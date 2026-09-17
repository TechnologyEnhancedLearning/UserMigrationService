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
}