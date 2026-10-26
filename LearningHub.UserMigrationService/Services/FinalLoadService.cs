using System.Data;
using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LearningHub.UserMigrationService.Services;

public sealed class FinalLoadService : IFinalLoadService
{
    private static readonly HashSet<string> AllowedProcedures = new(StringComparer.OrdinalIgnoreCase)
    {
        "migrations.MergeUsers",
        "migrations.MergeOrganisations",
        "migrations.MergeUserOrganisations",
        "migrations.MergeUserRoles",
        "migrations.MergeUserExternalSystems",
        "migrations.MergeUserPasswordValidationTokens",
        "migrations.MergeUserLearningHistoryLogs"
    };

    private readonly string _connectionString;
    private readonly ILogger<FinalLoadService> _logger;

    public FinalLoadService(IOptions<DatabaseOptions> databaseOptions, ILogger<FinalLoadService> logger)
    {
        _connectionString = databaseOptions.Value.LearningHubConnectionString;
        _logger = logger;
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("DatabaseOptions:LearningHubConnectionString is not configured.");
    }

    public async Task<FinalLoadResult> ExecuteAsync(string procedureName, Guid migrationRunId, CancellationToken cancellationToken)
    {
        if (!AllowedProcedures.Contains(procedureName))
            throw new ArgumentException($"Stored procedure '{procedureName}' is not allowed.", nameof(procedureName));

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand($"[{procedureName.Replace(".", "].[", StringComparison.Ordinal)}]", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 0
        };
        command.Parameters.Add("@MigrationRunId", SqlDbType.UniqueIdentifier).Value = migrationRunId;
        var read = AddOutput(command, "@RecordsRead");
        var inserted = AddOutput(command, "@RecordsInserted");
        var updated = AddOutput(command, "@RecordsUpdated");
        var skipped = AddOutput(command, "@RecordsSkipped");
        var failed = AddOutput(command, "@RecordsFailed");

        _logger.LogInformation("Executing {ProcedureName} for migration run {MigrationRunId}.", procedureName, migrationRunId);
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
            var result = new FinalLoadResult
            {
                RecordsRead = GetValue(read), RecordsInserted = GetValue(inserted),
                RecordsUpdated = GetValue(updated), RecordsSkipped = GetValue(skipped),
                RecordsFailed = GetValue(failed)
            };
            _logger.LogInformation("{ProcedureName} completed. Read={Read}, inserted={Inserted}, updated={Updated}, skipped={Skipped}, failed={Failed}.",
                procedureName, result.RecordsRead, result.RecordsInserted, result.RecordsUpdated, result.RecordsSkipped, result.RecordsFailed);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ProcedureName} failed for migration run {MigrationRunId}.", procedureName, migrationRunId);
            throw;
        }
    }

    private static SqlParameter AddOutput(SqlCommand command, string name)
    {
        var p = command.Parameters.Add(name, SqlDbType.BigInt);
        p.Direction = ParameterDirection.Output;
        return p;
    }

    private static long GetValue(SqlParameter p) => p.Value is null or DBNull ? 0 : Convert.ToInt64(p.Value);
}
