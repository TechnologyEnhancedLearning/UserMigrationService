using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Models.Extraction;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace LearningHub.UserMigrationService.Extractors;

public class UserExtractor : IUserExtractor
{
    private readonly string _connectionString;

    public UserExtractor(IOptions<DatabaseOptions> options)
    {
        _connectionString =
            options.Value.LegacyHubConnectionString;
    }

    public async IAsyncEnumerable<ElfhUser> ExtractAsync(
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

        const string sqlTemplate = """
            SELECT
                 u.userId
            ,u.firstName
            ,u.lastName
            ,u.emailAddress
            ,u.altEmailAddress
            ,u.userName
            ,u.registrationCode
            ,u.active
            ,u.passwordHash
            ,u.mustChangeNextLogin
            ,u.passwordLifeCounter
            ,u.RemoteLoginKey
            ,u.RemoteLoginGuid
            ,u.RemoteLoginStart
            ,u.RestrictToSSO
            ,u.createdDate
            ,u.amendDate
            ,u.amendUserId
            ,u.deleted
            FROM dbo.userTBL AS u
            WHERE u.userId IN ({0})
            ORDER BY u.userId;
            """;

        var sql = string.Format(
            sqlTemplate,
            parameterNames);

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

        var deletedOrdinal =
            reader.GetOrdinal("deleted");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ElfhUser
            {
                UserId =
                    reader.GetInt32(userIdOrdinal),

                Deleted =
                    reader.GetBoolean(deletedOrdinal),

            };
        }
    }
}