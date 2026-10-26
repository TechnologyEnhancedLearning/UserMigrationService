using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Data;
using LearningHub.UserMigrationService.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace LearningHub.UserMigrationService.Repositories
{
    public class LegacyRepository : ILegacyRepository
    {
        private readonly string _connectionString;
        private readonly UserMigrationDbContext _context;
        private readonly int _sqlCommandTimeoutSeconds;

        public LegacyRepository(IOptions<DatabaseOptions> options,
             IOptions<MigrationOptions> migrationOptions,
            UserMigrationDbContext context)
        {
            _connectionString = options.Value.LegacyHubConnectionString;
            _context = context;
            _sqlCommandTimeoutSeconds =migrationOptions.Value.SqlCommandTimeoutSeconds;
        }

        public async Task<bool> TestConnectionAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync();

            return connection.State == ConnectionState.Open;
        }
        public async Task<List<int>> GetElfhUsersToMigrateAsync(int months,CancellationToken cancellationToken)
        {
            const string sql = """
                                    SELECT DISTINCT
                                        u.userId
                                    FROM dbo.userTBL AS u
                                    WHERE
                                        u.deleted = 0
                                        AND u.preferredTenantId <> 2
                                        AND EXISTS
                                        (
                                            SELECT 1
                                            FROM dbo.userHistoryTBL AS uh
                                            WHERE
                                                uh.userId = u.userId
                                                AND uh.userHistoryTypeId = 0
                                                AND uh.createdDate >= DATEADD(
                                                    MONTH,
                                                    -@months,
                                                    SYSUTCDATETIME())
                                        );
                                    """;

            var userIds = new List<int>();

            using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command =    new SqlCommand(sql, connection)
            {
                CommandTimeout = _sqlCommandTimeoutSeconds
            };

            command.Parameters.Add(
                new SqlParameter("@months", SqlDbType.Int)
                {
                    Value = months
                });

            using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                userIds.Add(reader.GetInt32(0));
            }

            return userIds;
        }
        public async Task<List<int>> GetElfhAdminLocationIdsToMigrateAsync(IEnumerable<int> userIds,CancellationToken cancellationToken)
        {
            var ids = userIds
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return new List<int>();
            }

            var parameters = ids
                .Select((id, index) => new SqlParameter($"@UserId{index}", SqlDbType.Int)
                {
                    Value = id
                })
                .ToArray();

            var parameterNames = string.Join(
                ", ",
                parameters.Select(p => p.ParameterName));

                    var sql = $"""
                SELECT DISTINCT
                    ual.adminLocationId
                FROM dbo.userAdminLocationTBL AS ual
                WHERE ual.deleted = 0
                  AND ual.adminLocationId IS NOT NULL
                  AND ual.userId IN ({parameterNames});
                """;

            var locationIds = new List<int>();

            using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command =
                    new SqlCommand(sql, connection)
                    {
                        CommandTimeout = _sqlCommandTimeoutSeconds
                    };

            command.Parameters.AddRange(parameters);

            using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                locationIds.Add(reader.GetInt32(0));
            }

            return locationIds;
        }
        public async Task<List<int>> GetElfhEmploymentLocationIdsToMigrateAsync(IEnumerable<int> userIds,CancellationToken cancellationToken)
        {
            var ids = userIds
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return new List<int>();
            }

            var parameters = ids
                .Select((id, index) => new SqlParameter($"@UserId{index}", SqlDbType.Int)
                {
                    Value = id
                })
                .ToArray();

            var parameterNames = string.Join(
                ", ",
                parameters.Select(p => p.ParameterName));

                        var sql = $"""
                    SELECT
                        ue.locationId
                    FROM dbo.userEmploymentTBL AS ue
                    WHERE ue.deleted = 0
                      AND ue.locationId IS NOT NULL
                      AND ue.userId IN ({parameterNames})
                    GROUP BY ue.locationId
                    HAVING COUNT(*) > 0;
                    """;

            var locationIds = new List<int>();

            using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command =
                    new SqlCommand(sql, connection)
                    {
                        CommandTimeout = _sqlCommandTimeoutSeconds
                    };

            command.Parameters.AddRange(parameters);

            using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                locationIds.Add(reader.GetInt32(0));
            }

            return locationIds;
        }
        public async Task<Dictionary<int, int?>> GetInferredOrganisationRegionsAsync(
    IEnumerable<int> locationIds,
    CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(locationIds);

            var ids = locationIds
                .Distinct()
                .ToArray();

            // Return an entry for every requested location.
            // Locations without a qualifying region remain null.
            var result = ids.ToDictionary(
                locationId => locationId,
                _ => (int?)null);

            if (ids.Length == 0)
            {
                return result;
            }

            var locationIdsJson = JsonSerializer.Serialize(ids);

            const string sql = """
        WITH RequestedLocations AS
        (
            SELECT CAST([value] AS INT) AS LocationId
            FROM OPENJSON(@LocationIdsJson)
        ),
        LocationUsers AS
        (
            -- Users associated through admin locations.
            SELECT DISTINCT
                ual.adminLocationId AS LocationId,
                ual.userId AS UserId
            FROM dbo.userAdminLocationTBL AS ual
            INNER JOIN RequestedLocations AS rl
                ON rl.LocationId = ual.adminLocationId
            WHERE ual.deleted = 0
              AND ual.adminLocationId IS NOT NULL

            UNION

            -- Users associated through employment locations.
            SELECT DISTINCT
                ue.locationId AS LocationId,
                ue.userId AS UserId
            FROM dbo.userEmploymentTBL AS ue
            INNER JOIN RequestedLocations AS rl
                ON rl.LocationId = ue.locationId
            WHERE ue.deleted = 0
              AND ue.locationId IS NOT NULL
        ),
        RankedUsers AS
        (
            SELECT
                lu.LocationId,
                u.RegionId,
                ROW_NUMBER() OVER
                (
                    PARTITION BY lu.LocationId
                    ORDER BY lu.UserId
                ) AS UserNumber
            FROM LocationUsers AS lu
            INNER JOIN dbo.userTBL AS u
                ON u.userId  = lu.UserId
        ),
        RegionCounts AS
        (
            SELECT
                LocationId,
                RegionId,
                COUNT(*) AS UserCount
            FROM RankedUsers
            WHERE UserNumber <= 10
              AND RegionId IS NOT NULL
            GROUP BY
                LocationId,
                RegionId
        ),
        RankedRegions AS
        (
            SELECT
                LocationId,
                RegionId,
                UserCount,
                DENSE_RANK() OVER
                (
                    PARTITION BY LocationId
                    ORDER BY UserCount DESC
                ) AS RegionRank
            FROM RegionCounts
        ),
        WinningRegions AS
        (
            SELECT
                LocationId,
                RegionId,
                UserCount,
                COUNT(*) OVER
                (
                    PARTITION BY LocationId
                ) AS NumberOfTiedRegions
            FROM RankedRegions
            WHERE RegionRank = 1
        )
        SELECT
            LocationId,
            RegionId
        FROM WinningRegions
        WHERE UserCount >= 5
          AND NumberOfTiedRegions = 1;
        """;

            await using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, connection);

            command.Parameters.Add(
                "@LocationIdsJson",
                SqlDbType.NVarChar,
                -1).Value = locationIdsJson;

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var locationId = reader.GetInt32(0);
                var regionId = reader.GetInt32(1);

                result[locationId] = regionId;
            }

            return result;
        }
    }
}
