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
using System.Threading.Tasks;
namespace LearningHub.UserMigrationService.Repositories
{
    public class LegacyRepository : ILegacyRepository
    {
        private readonly string _connectionString;
        private readonly UserMigrationDbContext _context;

        public LegacyRepository(IOptions<DatabaseOptions> options,
            UserMigrationDbContext context)
        {
            _connectionString = options.Value.LegacyHubConnectionString;
            _context = context;
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

            using var command = new SqlCommand(sql, connection);

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

            using var command = new SqlCommand(sql, connection);

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

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddRange(parameters);

            using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                locationIds.Add(reader.GetInt32(0));
            }

            return locationIds;
        }
    }
}
