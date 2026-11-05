
using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models.Transformation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LearningHub.UserMigrationService.Repositories;

public class OrganisationTypeMappingRepository
    : IOrganisationTypeMappingRepository
{
    private const int FallbackOrganisationTypeId = 23;

    private readonly string _connectionString;

    public OrganisationTypeMappingRepository(
        IOptions<DatabaseOptions> databaseOptions)
    {
        _connectionString =
            databaseOptions.Value.LearningHubConnectionString;
    }

    public async Task<IReadOnlyList<OrganisationTypeMapping>>
        GetMappingsAsync(
            CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF NOT EXISTS
            (
                SELECT 1
                FROM [hub].[OrganisationType]
                WHERE [Id] = @FallbackOrganisationTypeId
            )
            BEGIN
                THROW 50001,
                    'Fallback OrganisationTypeId 23 does not exist in hub.OrganisationType.',
                    1;
            END;

            SELECT
                mapping.Id,
                mapping.LegacyOrganisationTypeId,
                mapping.LegacyOrganisationType,

                CASE
                    WHEN mapping.MappingConfidence = 3
                        THEN @FallbackOrganisationTypeId

                    WHEN destination.Id IS NULL
                        THEN @FallbackOrganisationTypeId

                    ELSE mapping.OrganisationTypeId
                END AS OrganisationTypeId,

                mapping.OrganisationType,
                mapping.MappingConfidence,
                mapping.CreatedUtc,
                mapping.UpdatedUtc

            FROM [migrations].[OrganisationTypeMapping] mapping

            LEFT JOIN [hub].[OrganisationType] destination
                ON destination.Id = mapping.OrganisationTypeId

            ORDER BY mapping.LegacyOrganisationTypeId;
            """;

        var results = new List<OrganisationTypeMapping>();

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = 0
        };

        command.Parameters.AddWithValue(
            "@FallbackOrganisationTypeId",
            FallbackOrganisationTypeId);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new OrganisationTypeMapping
            {
                Id = reader.GetInt32(
                    reader.GetOrdinal("Id")),

                LegacyOrganisationTypeId = reader.GetInt32(
                    reader.GetOrdinal("LegacyOrganisationTypeId")),

                LegacyOrganisationType = reader.GetString(
                    reader.GetOrdinal("LegacyOrganisationType")),

                OrganisationTypeId = reader.GetInt32(
                    reader.GetOrdinal("OrganisationTypeId")),

                OrganisationType = reader.GetString(
                    reader.GetOrdinal("OrganisationType")),

                MappingConfidence = reader.GetInt32(
                    reader.GetOrdinal("MappingConfidence")),

                CreatedUtc = reader.GetDateTime(
                    reader.GetOrdinal("CreatedUtc")),

                UpdatedUtc = reader.IsDBNull(
                    reader.GetOrdinal("UpdatedUtc"))
                    ? null
                    : reader.GetDateTime(
                        reader.GetOrdinal("UpdatedUtc"))
            });
        }

        return results;
    }
}
