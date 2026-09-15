using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Models.Extraction;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace LearningHub.UserMigrationService.Extractors;

public class OrganisationExtractor : IOrganisationExtractor
{
    private readonly string _connectionString;

    public OrganisationExtractor(
        IOptions<DatabaseOptions> options)
    {
        _connectionString =
            options.Value.LegacyHubConnectionString;
    }

    public async IAsyncEnumerable<ElfhOrganisation> ExtractAsync(
        IEnumerable<int> locationIds,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var ids = locationIds
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
                "LocationId");

        var sql = $"""
            SELECT
                locationId,
                locationCode,
                locationName,
                postCode,
                locationTypeId,
                parentId,
                created,
                updated
                --,amendUserId
                --,deleted
            FROM dbo.locationTBL AS l
            WHERE
                l.locationId IN ({parameterNames})
            ORDER BY
                l.locationId;
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

        var locationIdOrdinal =
            reader.GetOrdinal("locationId");

        var locationCodeOrdinal =
            reader.GetOrdinal("locationCode");

        var locationNameOrdinal =
            reader.GetOrdinal("locationName");

        var postCodeOrdinal =
            reader.GetOrdinal("postCode");

        var locationTypeIdOrdinal =
            reader.GetOrdinal("locationTypeId");

        var parentIdOrdinal =
            reader.GetOrdinal("parentId");

        var createdOrdinal =
            reader.GetOrdinal("created");

        var updatedOrdinal =
            reader.GetOrdinal("updated");

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new ElfhOrganisation
            {
                LocationId =
                    reader.GetInt32(locationIdOrdinal),

                LocationCode =
                    reader.IsDBNull(locationCodeOrdinal)
                        ? null
                        : reader.GetString(locationCodeOrdinal),

                LocationName =
                    reader.IsDBNull(locationNameOrdinal)
                        ? null
                        : reader.GetString(locationNameOrdinal),

                PostCode =
                    reader.IsDBNull(postCodeOrdinal)
                        ? null
                        : reader.GetString(postCodeOrdinal),

                LocationTypeId =
                    reader.IsDBNull(locationTypeIdOrdinal)
                        ? null
                        : reader.GetInt32(locationTypeIdOrdinal),

                ParentId =
                    reader.IsDBNull(parentIdOrdinal)
                        ? null
                        : reader.GetInt32(parentIdOrdinal),

                Created =
                    reader.IsDBNull(createdOrdinal)
                        ? null
                        : reader.GetDateTime(createdOrdinal),

                Updated =
                    reader.IsDBNull(updatedOrdinal)
                        ? null
                        : reader.GetDateTime(updatedOrdinal)
            };
        }
    }
}