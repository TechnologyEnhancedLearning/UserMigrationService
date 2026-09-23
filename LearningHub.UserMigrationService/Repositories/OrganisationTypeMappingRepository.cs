using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models.Transformation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LearningHub.UserMigrationService.Repositories;

public class OrganisationTypeMappingRepository
    : IOrganisationTypeMappingRepository
{
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
            SELECT
                Id,
                LegacyOrganisationTypeId,
                LegacyOrganisationType,
                OrganisationTypeId,
                OrganisationType,
                IsMapped,
                CreatedUtc,
                UpdatedUtc
            FROM [migrations].[OrganisationTypeMapping]
            ORDER BY
                LegacyOrganisationTypeId;
            """;

        var results =
            new List<OrganisationTypeMapping>();

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(
            cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = 0
            };

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        var idOrdinal =
            reader.GetOrdinal("Id");

        var legacyIdOrdinal =
            reader.GetOrdinal(
                "LegacyOrganisationTypeId");

        var legacyNameOrdinal =
            reader.GetOrdinal(
                "LegacyOrganisationType");

        var organisationTypeIdOrdinal =
            reader.GetOrdinal(
                "OrganisationTypeId");

        var organisationTypeOrdinal =
            reader.GetOrdinal(
                "OrganisationType");

        var isMappedOrdinal =
            reader.GetOrdinal("IsMapped");

        var createdUtcOrdinal =
            reader.GetOrdinal("CreatedUtc");

        var updatedUtcOrdinal =
            reader.GetOrdinal("UpdatedUtc");

        while (await reader.ReadAsync(
            cancellationToken))
        {
            results.Add(
                new OrganisationTypeMapping
                {
                    Id =
                        reader.GetInt32(
                            idOrdinal),

                    LegacyOrganisationTypeId =
                        reader.GetInt32(
                            legacyIdOrdinal),

                    LegacyOrganisationType =
                        reader.IsDBNull(
                            legacyNameOrdinal)
                            ? null
                            : reader.GetString(
                                legacyNameOrdinal),

                    OrganisationTypeId =
                        reader.IsDBNull(
                            organisationTypeIdOrdinal)
                            ? null
                            : reader.GetInt32(
                                organisationTypeIdOrdinal),

                    OrganisationType =
                        reader.IsDBNull(
                            organisationTypeOrdinal)
                            ? null
                            : reader.GetString(
                                organisationTypeOrdinal),

                    IsMapped =
                        reader.GetBoolean(
                            isMappedOrdinal),

                    CreatedUtc =
                        reader.GetDateTime(
                            createdUtcOrdinal),

                    UpdatedUtc =
                        reader.IsDBNull(
                            updatedUtcOrdinal)
                            ? null
                            : reader.GetDateTime(
                                updatedUtcOrdinal)
                });
        }

        return results;
    }
}