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
        /*
         * Replace this query with the actual eLFH
         * Professional Body source query.
         */

        const string sql = """
            SELECT
                pb.professionalBodyId,
                pb.professionalBody
            FROM dbo.professionalBodyTBL AS pb
            ORDER BY
                pb.professionalBodyId;
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

        var idOrdinal =
            reader.GetOrdinal("professionalBodyId");

        var nameOrdinal =
            reader.GetOrdinal("professionalBody");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ElfhProfessionalBody
            {
                ProfessionalBodyId =
                    reader.GetInt32(idOrdinal),

                ProfessionalBody =
                    reader.IsDBNull(nameOrdinal)
                        ? null
                        : reader.GetString(nameOrdinal)
            };
        }
    }
}