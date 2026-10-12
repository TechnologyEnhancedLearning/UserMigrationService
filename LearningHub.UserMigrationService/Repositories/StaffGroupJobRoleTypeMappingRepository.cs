using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models.Transformation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LearningHub.UserMigrationService.Repositories;

public class StaffGroupJobRoleTypeMappingRepository
    : IStaffGroupJobRoleTypeMappingRepository
{
    private readonly string _connectionString;

    public StaffGroupJobRoleTypeMappingRepository(
        IOptions<DatabaseOptions> databaseOptions)
    {
        _connectionString =
            databaseOptions.Value.LearningHubConnectionString;
    }

public async Task<IReadOnlyList<StaffGroupJobRoleTypeMapping>>
    GetMappingsAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
        SELECT
            Id,
            LegacyStaffGroupId,
            LegacyStaffGroup,
            JobRoleTypeId,
            JobRoleType,
            IsMapped,
            CreatedUtc,
            UpdatedUtc
        FROM [migrations].[StaffGroupJobRoleTypeMapping]
        ORDER BY LegacyStaffGroupId;
        """;

        var results = new List<StaffGroupJobRoleTypeMapping>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = 0
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var idOrdinal = reader.GetOrdinal("Id");
        var legacyIdOrdinal = reader.GetOrdinal("LegacyStaffGroupId");
        var legacyNameOrdinal = reader.GetOrdinal("LegacyStaffGroup");
        var jobRoleIdOrdinal = reader.GetOrdinal("JobRoleTypeId");
        var jobRoleNameOrdinal = reader.GetOrdinal("JobRoleType");
        var isMappedOrdinal = reader.GetOrdinal("IsMapped");
        var createdUtcOrdinal = reader.GetOrdinal("CreatedUtc");

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new StaffGroupJobRoleTypeMapping
            {
                Id = reader.GetInt32(idOrdinal),

                LegacyStaffGroupId =
                    reader.GetInt32(legacyIdOrdinal),

                LegacyStaffGroup =
                    reader.IsDBNull(legacyNameOrdinal)
                        ? string.Empty
                        : reader.GetString(legacyNameOrdinal),

                JobRoleTypeId =
                    reader.IsDBNull(jobRoleIdOrdinal)
                        ? null
                        : reader.GetInt32(jobRoleIdOrdinal),

                JobRoleType =
                    reader.IsDBNull(jobRoleNameOrdinal)
                        ? string.Empty
                        : reader.GetString(jobRoleNameOrdinal),

                IsMapped = !reader.IsDBNull(isMappedOrdinal)
                    && reader.GetBoolean(isMappedOrdinal),

                CreatedUtc = reader.GetDateTime(createdUtcOrdinal)
            });
        }

        return results;
    }
}