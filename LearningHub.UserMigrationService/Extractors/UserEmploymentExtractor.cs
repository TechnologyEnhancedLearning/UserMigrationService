using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Models.Extraction;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace LearningHub.UserMigrationService.Extractors;

public class UserEmploymentExtractor : IUserEmploymentExtractor
{
    private readonly string _connectionString;

    public UserEmploymentExtractor(
        IOptions<DatabaseOptions> options)
    {
        _connectionString =
            options.Value.LegacyHubConnectionString;
    }

    public async IAsyncEnumerable<ElfhUserEmployment> ExtractAsync(
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
                userEmploymentId
            ,userId
            ,locationId
            ,jobRoleId
            ,startDate
            ,endDate
            --,createdDate
            --,createUserId
            ,amendDate
            ,amendUserId
            ,deleted
            FROM dbo.userEmploymentTBL AS ue
            WHERE
                ue.userId IN ({parameterNames})
            ORDER BY
                ue.userId,
                ue.locationId;
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

        var userEmploymentIdOrdinal =
             reader.GetOrdinal("userEmploymentId");

        var userIdOrdinal =
            reader.GetOrdinal("userId");

        var locationIdOrdinal =
            reader.GetOrdinal("locationId");

        var jobRoleIdOrdinal =
            reader.GetOrdinal("jobRoleId");

        var startDateOrdinal =
            reader.GetOrdinal("startDate");

        var endDateOrdinal =
            reader.GetOrdinal("endDate");

        var amendDateOrdinal =
            reader.GetOrdinal("amendDate");

        var amendUserIdOrdinal =
            reader.GetOrdinal("amendUserId");

        var deletedOrdinal =
            reader.GetOrdinal("deleted");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ElfhUserEmployment
            {
                UserEmploymentId =
        reader.GetInt32(userEmploymentIdOrdinal),

                UserId =
        reader.GetInt32(userIdOrdinal),

                LocationId =
        reader.IsDBNull(locationIdOrdinal)
            ? null
            : reader.GetInt32(locationIdOrdinal),

                JobRoleId =
        reader.IsDBNull(jobRoleIdOrdinal)
            ? null
            : reader.GetInt32(jobRoleIdOrdinal),

                StartDate =
        reader.IsDBNull(startDateOrdinal)
            ? null
            : reader.GetDateTime(startDateOrdinal),

                EndDate =
        reader.IsDBNull(endDateOrdinal)
            ? null
            : reader.GetDateTime(endDateOrdinal),

                AmendDate =
        reader.IsDBNull(amendDateOrdinal)
            ? null
            : reader.GetDateTime(amendDateOrdinal),

                AmendUserId =
        reader.IsDBNull(amendUserIdOrdinal)
            ? null
            : reader.GetInt32(amendUserIdOrdinal),

                Deleted =
        reader.GetBoolean(deletedOrdinal)
            };
        }
    }
}