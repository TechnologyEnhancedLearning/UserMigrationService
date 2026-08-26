using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Data;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models;
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
    public class LearningHubRepository : ILearningHubRepository
    {
        private readonly string _connectionString;

        private readonly UserMigrationDbContext _context;

        public LearningHubRepository(IOptions<DatabaseOptions> options,UserMigrationDbContext context)
        {
            _connectionString =
                options.Value.LearningHubConnectionString;

            _context = context;
        }

        public async Task<bool> TestConnectionAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync();

            return connection.State == ConnectionState.Open;
        }
        public async Task<int> SaveUserIdsToMigrateAsync(IEnumerable<int> userIds, CancellationToken cancellationToken)
        {
            var ids = userIds
                .Distinct()
                .ToList();

            // Clear the previous migration scope.
            await _context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE migrations.UserIdsToMigrate",
                cancellationToken);

            var entities = ids.Select(id =>
                new UserIdToMigrate
                {
                    UserId = id
                });

            await _context.UserIdsToMigrate.AddRangeAsync(
                entities,
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);

            return ids.Count;
        }
        public async Task<List<int>> GetUsersWithExistingDataAsync(CancellationToken cancellationToken)
        {
            const string sql = """
                    SELECT DISTINCT UserId
                    FROM
                    (
                        SELECT UserId
                        FROM activity.MediaResourcePlayedSegment

                        UNION

                        SELECT UserId
                        FROM activity.NodeActivity

                        UNION

                        SELECT UserId
                        FROM activity.ResourceActivity

                        UNION

                        SELECT UserId
                        FROM analytics.Event

                        UNION

                        SELECT UserId
                        FROM hierarchy.CatalogueAccessRequest

                        UNION

                        SELECT UserId
                        FROM hub.EmailChangeValidationToken

                        UNION

                        SELECT UserId
                        FROM hub.UserBookmark

                        UNION

                        SELECT UserId
                        FROM hub.UserNotification

                        UNION

                        SELECT UserId
                        FROM hub.UserProvider

                        UNION

                        SELECT UserId
                        FROM messaging.MessageSendRecipient

                        UNION

                        SELECT UserId
                        FROM resources.ResourceVersionRating

                        UNION

                        SELECT UserId
                        FROM resources.ResourceVersionUserAcceptance

                        UNION

                        SELECT AuthorUserId AS UserId
                        FROM resources.ResourceVersionAuthor
                    ) AS UsersWithData
                    WHERE UserId IS NOT NULL;
                    """;

            var results = new List<int>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var command = new SqlCommand(sql, connection);

            using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(reader.GetInt32(0));
            }

            return results;
        }
        public async Task InsertUsersToMigrateAsync(IEnumerable<int> userIds,CancellationToken cancellationToken)
        {
            const string sql = """
                            INSERT INTO migrations.UserIdsToMigrate (UserId)
                            SELECT @UserId
                            WHERE NOT EXISTS
                            (
                                SELECT 1
                                FROM migrations.UserIdsToMigrate
                                WHERE UserId = @UserId
                            );
                            """;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            using var transaction =
                await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var userId in userIds.Distinct())
                {
                    using var command = new SqlCommand(sql, connection);

                    command.Transaction =
                        (SqlTransaction)transaction;

                    command.Parameters.AddWithValue("@UserId", userId);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        public async Task<List<int>> GetUserIdsToMigrateAsync(CancellationToken cancellationToken)
        {
            return await _context.UserIdsToMigrate
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<int>> GetElfhAdminLocationIdsToMigrateAsync(CancellationToken cancellationToken)
        {
            const string sql = """
                                SELECT DISTINCT
                                    ual.adminLocationId
                                FROM dbo.userAdminLocationTBL ual
                                INNER JOIN dbo.UserIdsToMigrate u
                                    ON u.UserId = ual.userId
                                WHERE ual.deleted = 0
                                  AND ual.adminLocationId IS NOT NULL;
                                """;

            return await _context.Database.SqlQueryRaw<int>(sql)
                        .ToListAsync(cancellationToken);
        }
        public async Task<int> InsertOrganisationLocationIdsToMigrateAsync( IEnumerable<int> locationIds, CancellationToken cancellationToken)
        {
            var ids = locationIds
                .Distinct()
                .ToList();

            await _context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE migrations.OrganisationLocationIdsToMigrate",
                cancellationToken);

            var entities = ids.Select(id =>
                new OrganisationLocationIdToMigrate
                {
                    LocationId = id
                });

            await _context.OrganisationLocationIdsToMigrate.AddRangeAsync(
                entities,
                cancellationToken);

            await _context.SaveChangesAsync(
                cancellationToken);

            return ids.Count;
        }
    }
}
