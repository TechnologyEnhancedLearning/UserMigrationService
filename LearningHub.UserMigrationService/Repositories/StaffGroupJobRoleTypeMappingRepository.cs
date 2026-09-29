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

        var results =
            new List<StaffGroupJobRoleTypeMapping>();

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(
                new StaffGroupJobRoleTypeMapping
                {
                    Id = reader.GetInt32(
                        reader.GetOrdinal("Id")),

                    LegacyStaffGroupId =
                        reader.GetInt32(
                            reader.GetOrdinal(
                                "LegacyStaffGroupId")),

                    LegacyStaffGroup =
                        reader.GetString(
                            reader.GetOrdinal(
                                "LegacyStaffGroup")),

                    JobRoleTypeId =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "JobRoleTypeId"))
                            ? null
                            : reader.GetInt32(
                                reader.GetOrdinal(
                                    "JobRoleTypeId")),

                    JobRoleType =
                        reader.GetString(
                            reader.GetOrdinal(
                                "JobRoleType")),

                    IsMapped =
                        reader.GetBoolean(
                            reader.GetOrdinal(
                                "IsMapped")),

                    CreatedUtc =
                        reader.GetDateTime(
                            reader.GetOrdinal(
                                "CreatedUtc")),

                    UpdatedUtc =
                        reader.IsDBNull(
                            reader.GetOrdinal(
                                "UpdatedUtc"))
                            ? null
                            : reader.GetDateTime(
                                reader.GetOrdinal(
                                    "UpdatedUtc"))
                });
        }

        return results;
    }
}