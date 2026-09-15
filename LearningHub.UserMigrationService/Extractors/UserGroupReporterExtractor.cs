using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Models.Extraction;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace LearningHub.UserMigrationService.Extractors;

public class UserGroupReporterExtractor : IUserGroupReporterExtractor
{
    private readonly string _connectionString;

    public UserGroupReporterExtractor(
        IOptions<DatabaseOptions> options)
    {
        _connectionString =
            options.Value.LegacyHubConnectionString;
    }

    public async IAsyncEnumerable<ElfhUserGroupReporter> ExtractAsync(
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
               ugr.userGroupReporterId
            ,ugr.userId
            ,ugr.userGroupId
            ,ugr.deleted
            ,ugr.amendUserId
            ,ugr.amendDate
            FROM dbo.userGroupReporterTBL AS ugr
            WHERE
                ugr.userId IN ({parameterNames})
            ORDER BY
                ugr.userId;
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

        var userGroupReporterIdOrdinal =
     reader.GetOrdinal("userGroupReporterId");

        var userIdOrdinal =
            reader.GetOrdinal("userId");

        var userGroupIdOrdinal =
            reader.GetOrdinal("userGroupId");

        var deletedOrdinal =
            reader.GetOrdinal("deleted");

        var amendUserIdOrdinal =
            reader.GetOrdinal("amendUserId");

        var amendDateOrdinal =
            reader.GetOrdinal("amendDate");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ElfhUserGroupReporter
            {
                UserGroupReporterId =
        reader.GetInt32(userGroupReporterIdOrdinal),

                UserId =
        reader.GetInt32(userIdOrdinal),

                UserGroupId =
        reader.GetInt32(userGroupIdOrdinal),

                Deleted =
        reader.GetBoolean(deletedOrdinal),

                AmendUserId =
        reader.IsDBNull(amendUserIdOrdinal)
            ? null
            : reader.GetInt32(amendUserIdOrdinal),

                AmendDate =
        reader.IsDBNull(amendDateOrdinal)
            ? null
            : reader.GetDateTime(amendDateOrdinal)
            };
        }
    }
}