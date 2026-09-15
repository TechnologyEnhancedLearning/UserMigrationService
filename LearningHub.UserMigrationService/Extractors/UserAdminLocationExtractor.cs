using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Models.Extraction;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace LearningHub.UserMigrationService.Extractors;

public class UserAdminLocationExtractor : IUserAdminLocationExtractor
{
    private readonly string _connectionString;

    public UserAdminLocationExtractor(
        IOptions<DatabaseOptions> options)
    {
        _connectionString =
            options.Value.LegacyHubConnectionString;
    }

    public async IAsyncEnumerable<ElfhUserAdminLocation> ExtractAsync(
        IEnumerable<int> userIds,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var ids = userIds
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            yield break;
        }

        var (
            parameters,
            parameterNames) =
            SqlParameterHelper.CreateIntParameters(
                ids,
                "UserId");

        var sql = $"""
            SELECT
                ual.userId,
                ual.adminLocationId,
                ual.deleted
            FROM dbo.userAdminLocationTBL AS ual
            WHERE
                ual.userId IN ({parameterNames})
            ORDER BY
                ual.userId,
                ual.adminLocationId;
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.CommandTimeout = 0;

        command.Parameters.AddRange(
            parameters.ToArray());

        await using var reader =
            await command.ExecuteReaderAsync(
                CommandBehavior.SequentialAccess,
                cancellationToken);

        var userIdOrdinal =
            reader.GetOrdinal("userId");

        var locationIdOrdinal =
            reader.GetOrdinal("adminLocationId");

        var deletedOrdinal =
            reader.GetOrdinal("deleted");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ElfhUserAdminLocation
            {
                UserId =
                    reader.GetInt32(userIdOrdinal),

                AdminLocationId =
                    reader.IsDBNull(locationIdOrdinal)
                        ? null
                        : reader.GetInt32(locationIdOrdinal),

                Deleted =
                    reader.GetBoolean(deletedOrdinal)
            };
        }
    }
}