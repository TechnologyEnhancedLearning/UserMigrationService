using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Models.Extraction;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace LearningHub.UserMigrationService.Extractors;

public class ProfessionalBodyExtractor : IProfessionalBodyExtractor
{
    private readonly string _connectionString;

    public ProfessionalBodyExtractor(
        IOptions<DatabaseOptions> options)
    {
        _connectionString =
            options.Value.LegacyHubConnectionString;
    }

    public async IAsyncEnumerable<ElfhProfessionalBody> ExtractAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                mc.medicalCouncilId,
                mc.medicalCouncilName,
                mc.medicalCouncilCode,
                mc.uploadPrefix,
                mc.includeOnCerts,
                mc.deleted,
                mc.amendUserID,
                mc.amendDate
            FROM dbo.medicalCouncilTBL AS mc
            ORDER BY
                mc.medicalCouncilId;
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

        var medicalCouncilIdOrdinal =
            reader.GetOrdinal("medicalCouncilId");

        var medicalCouncilNameOrdinal =
            reader.GetOrdinal("medicalCouncilName");

        var medicalCouncilCodeOrdinal =
            reader.GetOrdinal("medicalCouncilCode");

        var uploadPrefixOrdinal =
            reader.GetOrdinal("uploadPrefix");

        var includeOnCertsOrdinal =
            reader.GetOrdinal("includeOnCerts");

        var deletedOrdinal =
            reader.GetOrdinal("deleted");

        var amendUserIdOrdinal =
            reader.GetOrdinal("amendUserID");

        var amendDateOrdinal =
            reader.GetOrdinal("amendDate");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ElfhProfessionalBody
            {
                ProfessionalBodyId =
                    reader.GetInt32(medicalCouncilIdOrdinal),

                ProfessionalBody =
                    reader.IsDBNull(medicalCouncilNameOrdinal)
                        ? null
                        : reader.GetString(medicalCouncilNameOrdinal),

                ProfessionalBodyCode =
                    reader.IsDBNull(medicalCouncilCodeOrdinal)
                        ? null
                        : reader.GetString(medicalCouncilCodeOrdinal),

                UploadPrefix =
                    reader.IsDBNull(uploadPrefixOrdinal)
                        ? null
                        : reader.GetString(uploadPrefixOrdinal),

                IncludeOnCerts =
                    !reader.IsDBNull(includeOnCertsOrdinal) &&
                    reader.GetBoolean(includeOnCertsOrdinal),

                Deleted =
                    !reader.IsDBNull(deletedOrdinal) &&
                    reader.GetBoolean(deletedOrdinal),

                AmendUserId =
                    reader.IsDBNull(amendUserIdOrdinal)
                        ? null
                        : reader.GetInt32(amendUserIdOrdinal),

                AmendDate =
                    reader.IsDBNull(amendDateOrdinal)
                        ? null
                        : reader.GetFieldValue<DateTimeOffset>(amendDateOrdinal)
            };
        }
    }
}