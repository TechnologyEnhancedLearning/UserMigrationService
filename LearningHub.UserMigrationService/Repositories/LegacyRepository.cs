using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using Microsoft.Data.SqlClient;
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

        public LegacyRepository(IOptions<DatabaseOptions> options)
        {
            _connectionString = options.Value.LegacyHubConnectionString;
        }

        public async Task<bool> TestConnectionAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync();

            return connection.State == ConnectionState.Open;
        }
    }
}
