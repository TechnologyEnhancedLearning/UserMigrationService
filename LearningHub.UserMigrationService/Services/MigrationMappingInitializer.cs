using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LearningHub.UserMigrationService.Services;

public class MigrationMappingInitializer
    : IMigrationMappingInitializer
{
    private readonly string _connectionString;

    private readonly IProfessionalBodyExtractor  _professionalBodyExtractor;

    private readonly ISupportingLookupExtractor  _supportingLookupExtractor;

    public MigrationMappingInitializer(
        IOptions<DatabaseOptions> databaseOptions,
        IProfessionalBodyExtractor professionalBodyExtractor,
        ISupportingLookupExtractor supportingLookupExtractor)
    {
        _connectionString = databaseOptions.Value.LearningHubConnectionString;

        _professionalBodyExtractor = professionalBodyExtractor;

        _supportingLookupExtractor = supportingLookupExtractor;
    }

    public async Task InitialiseAsync(
        CancellationToken cancellationToken = default)
    {
        await InitialiseProfessionalBodyMappingsAsync(
            cancellationToken);

        await InitialiseOrganisationTypeMappingsAsync(
            cancellationToken);

        await InitialiseUserMappingsAsync(
       cancellationToken);

        await InitialiseOrganisationMappingsAsync(
            cancellationToken);
    }


    private async Task InitialiseProfessionalBodyMappingsAsync(
    CancellationToken cancellationToken)
    {
        const string existsSql = """
        SELECT COUNT(1)
        FROM [migrations].[ProfessionalBodyMapping]
        WHERE LegacyProfessionalBodyId =
              @LegacyProfessionalBodyId;
        """;

        const string insertSql = """
        INSERT INTO [migrations].[ProfessionalBodyMapping]
        (
            LegacyProfessionalBodyId,
            LegacyProfessionalBody,
            ProfessionalBodyId,
            ProfessionalBody,
            IsMapped,
            CreatedUtc
        )
        VALUES
        (
            @LegacyProfessionalBodyId,
            @LegacyProfessionalBody,
            NULL,
            @ProfessionalBody,
            0,
            SYSUTCDATETIME()
        );
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(
            cancellationToken);

        await foreach (
            var source in
            _professionalBodyExtractor
                .ExtractAsync(cancellationToken)
                .WithCancellation(cancellationToken))
        {
            if (source.ProfessionalBodyId <= 0)
            {
                continue;
            }

            var legacyProfessionalBody =
                string.IsNullOrWhiteSpace(source.ProfessionalBody)
                    ? $"Legacy Professional Body {source.ProfessionalBodyId}"
                    : source.ProfessionalBody.Trim();

            await using var existsCommand =
                new SqlCommand(
                    existsSql,
                    connection);

            existsCommand.Parameters.Add(
                new SqlParameter(
                    "@LegacyProfessionalBodyId",
                    System.Data.SqlDbType.Int)
                {
                    Value = source.ProfessionalBodyId
                });

            var exists =
                Convert.ToInt32(
                    await existsCommand.ExecuteScalarAsync(
                        cancellationToken));

            if (exists > 0)
            {
                continue;
            }

            await using var insertCommand =
                new SqlCommand(
                    insertSql,
                    connection);

            insertCommand.Parameters.Add(
                new SqlParameter(
                    "@LegacyProfessionalBodyId",
                    System.Data.SqlDbType.Int)
                {
                    Value = source.ProfessionalBodyId
                });

            insertCommand.Parameters.Add(
                new SqlParameter(
                    "@LegacyProfessionalBody",
                    System.Data.SqlDbType.NVarChar,
                    100)
                {
                    Value = legacyProfessionalBody
                });

            insertCommand.Parameters.Add(
                new SqlParameter(
                    "@ProfessionalBody",
                    System.Data.SqlDbType.NVarChar,
                    100)
                {
                    Value = legacyProfessionalBody
                });

            await insertCommand.ExecuteNonQueryAsync(
                cancellationToken);
        }
    }
    private async Task InitialiseOrganisationTypeMappingsAsync(
        CancellationToken cancellationToken)
    {
        const string existsSql = """
            SELECT COUNT(1)
            FROM [migrations].[OrganisationTypeMapping]
            WHERE LegacyOrganisationTypeId =
                  @LegacyOrganisationTypeId;
            """;

        const string insertSql = """
            INSERT INTO [migrations].[OrganisationTypeMapping]
            (
                LegacyOrganisationTypeId,
                LegacyOrganisationType,
                OrganisationTypeId,
                OrganisationType,
                IsMapped,
                CreatedUtc
            )
            VALUES
            (
                @LegacyOrganisationTypeId,
                @LegacyOrganisationType,
                NULL,
                @LegacyOrganisationType,
                0,
                SYSUTCDATETIME()
            );
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(
            cancellationToken);

        await foreach (
            var source in
            _supportingLookupExtractor
                .ExtractOrganisationTypesAsync(
                    cancellationToken)
                .WithCancellation(cancellationToken))
        {
            // Ignore invalid organisation types.
            if (source.Id <= 0 ||
                string.IsNullOrWhiteSpace(source.Name))
            {
                continue;
            }

            var legacyOrganisationType =
                source.Name.Trim();

            await using var existsCommand =
                new SqlCommand(
                    existsSql,
                    connection);

            existsCommand.Parameters.Add(
                new SqlParameter(
                    "@LegacyOrganisationTypeId",
                    System.Data.SqlDbType.Int)
                {
                    Value = source.Id
                });

            var exists =
                Convert.ToInt32(
                    await existsCommand.ExecuteScalarAsync(
                        cancellationToken));

            if (exists > 0)
            {
                continue;
            }

            await using var insertCommand =
                new SqlCommand(
                    insertSql,
                    connection);

            insertCommand.Parameters.Add(
                new SqlParameter(
                    "@LegacyOrganisationTypeId",
                    System.Data.SqlDbType.Int)
                {
                    Value = source.Id
                });

            insertCommand.Parameters.Add(
                new SqlParameter(
                    "@LegacyOrganisationType",
                    System.Data.SqlDbType.NVarChar,
                    100)
                {
                    Value = legacyOrganisationType
                });

            await insertCommand.ExecuteNonQueryAsync(
                cancellationToken);
        }
    }
    private async Task InitialiseUserMappingsAsync(
    CancellationToken cancellationToken)
    {
        const string sql = """
        INSERT INTO [migrations].[UserMapping]
        (
            LegacyUserId,
            UserId,
            MatchType,
            IsMapped,
            CreatedUtc
        )
        SELECT DISTINCT
            u.UserId,
            NULL,
            NULL,
            0,
            SYSUTCDATETIME()
        FROM [migrations].[UserIdsToMigrate] AS u
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [migrations].[UserMapping] AS m
            WHERE m.LegacyUserId = u.UserId
        );
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(
            cancellationToken);

        await using var command =
            new SqlCommand(
                sql,
                connection);

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }
    private async Task InitialiseOrganisationMappingsAsync(
    CancellationToken cancellationToken)
    {
        const string sql = """
        INSERT INTO [migrations].[OrganisationMapping]
        (
            LegacyOrganisationId,
            OrganisationId,
            MatchType,
            IsMapped,
            CreatedUtc
        )
        SELECT
            o.LocationId,
            NULL,
            NULL,
            0,
            SYSUTCDATETIME()
        FROM [migrations].[OrganisationLocationIdsToMigrate] AS o
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM [migrations].[OrganisationMapping] AS m
            WHERE m.LegacyOrganisationId = o.LocationId
        );
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        await command.ExecuteNonQueryAsync(
            cancellationToken);
    }
}