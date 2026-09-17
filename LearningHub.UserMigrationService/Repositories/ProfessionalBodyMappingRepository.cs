using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models.Transformation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LearningHub.UserMigrationService.Repositories;

public class ProfessionalBodyMappingRepository
    : IProfessionalBodyMappingRepository
{
    private readonly string _connectionString;

    public ProfessionalBodyMappingRepository(
        IOptions<DatabaseOptions> databaseOptions)
    {
        _connectionString =
            databaseOptions.Value.LearningHubConnectionString;
    }

    public async Task<IReadOnlyList<ProfessionalBodyMapping>> GetMappingsAsync(
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                Id,
                LegacyProfessionalBodyId,
                LegacyProfessionalBody,
                ProfessionalBodyId,
                ProfessionalBody,
                IsMapped,
                CreatedUtc,
                UpdatedUtc
            FROM [migrations].[ProfessionalBodyMapping]
            ORDER BY LegacyProfessionalBodyId;
            """;

        var results = new List<ProfessionalBodyMapping>();

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
            results.Add(new ProfessionalBodyMapping
            {
                Id = reader.GetInt32(
                    reader.GetOrdinal("Id")),

                LegacyProfessionalBodyId = reader.GetInt32(
                    reader.GetOrdinal("LegacyProfessionalBodyId")),

                LegacyProfessionalBody =
                    reader["LegacyProfessionalBody"] as string,

                ProfessionalBodyId =
                    reader["ProfessionalBodyId"] == DBNull.Value
                        ? null
                        : reader.GetInt32(
                            reader.GetOrdinal("ProfessionalBodyId")),

                ProfessionalBody =
                    reader["ProfessionalBody"] as string,

                IsMapped = reader.GetBoolean(
                    reader.GetOrdinal("IsMapped")),

                CreatedUtc = reader.GetDateTime(
                    reader.GetOrdinal("CreatedUtc")),

                UpdatedUtc =
                    reader["UpdatedUtc"] == DBNull.Value
                        ? null
                        : reader.GetDateTime(
                            reader.GetOrdinal("UpdatedUtc"))
            });
        }

        return results;
    }
}