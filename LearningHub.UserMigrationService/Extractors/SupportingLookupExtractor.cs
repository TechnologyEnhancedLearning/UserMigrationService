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

    public async IAsyncEnumerable<ElfhSupportingLookup> ExtractAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        /*
         * Add the actual supporting lookup UNION/query here
         * once the source tables and columns are confirmed.
         */

        const string sql = """
            /*
             * Example only.
             *
             * SELECT
             *     'LookupType' AS LookupType,
             *     id,
             *     name,
             *     description
             * FROM dbo.SomeLookupTBL
             */
            SELECT
                CAST(NULL AS NVARCHAR(100)) AS LookupType,
                CAST(NULL AS INT) AS Id,
                CAST(NULL AS NVARCHAR(255)) AS Name,
                CAST(NULL AS NVARCHAR(500)) AS Description
            WHERE 1 = 0;
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

        var lookupTypeOrdinal =
            reader.GetOrdinal("LookupType");

        var idOrdinal =
            reader.GetOrdinal("Id");

        var nameOrdinal =
            reader.GetOrdinal("Name");

        var descriptionOrdinal =
            reader.GetOrdinal("Description");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ElfhSupportingLookup
            {
                LookupType =
                    reader.IsDBNull(lookupTypeOrdinal)
                        ? string.Empty
                        : reader.GetString(lookupTypeOrdinal),

                Id =
                    reader.GetInt32(idOrdinal),

                Name =
                    reader.IsDBNull(nameOrdinal)
                        ? null
                        : reader.GetString(nameOrdinal),

                Description =
                    reader.IsDBNull(descriptionOrdinal)
                        ? null
                        : reader.GetString(descriptionOrdinal)
            };
        }
    }
}