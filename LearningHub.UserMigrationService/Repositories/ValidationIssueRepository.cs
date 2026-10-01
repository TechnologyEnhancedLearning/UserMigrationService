using System.Data;
using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LearningHub.UserMigrationService.Repositories;

public class ValidationIssueRepository : IValidationIssueRepository
{
    private readonly string _connectionString;

    public ValidationIssueRepository(
        IOptions<DatabaseOptions> databaseOptions)
    {
        _connectionString =
            databaseOptions.Value.LearningHubConnectionString;
    }

    public async Task InsertAsync(
        IReadOnlyCollection<ValidationIssue> issues,
        CancellationToken cancellationToken = default)
    {
        if (issues.Count == 0)
            return;

        var table = new DataTable();

        table.Columns.Add("MigrationRunId", typeof(Guid));
        table.Columns.Add("TableName", typeof(string));
        table.Columns.Add("StagingRecordId", typeof(long));
        table.Columns.Add("ElfhRecordId", typeof(int));
        table.Columns.Add("ValidationType", typeof(string));
        table.Columns.Add("Severity", typeof(string));
        table.Columns.Add("ColumnName", typeof(string));
        table.Columns.Add("Message", typeof(string));
        table.Columns.Add("CreatedUtc", typeof(DateTime));

        foreach (var issue in issues)
        {
            table.Rows.Add(
                issue.MigrationRunId,
                issue.TableName,
                issue.StagingRecordId ?? (object)DBNull.Value,
                issue.ElfhRecordId ?? (object)DBNull.Value,
                issue.ValidationType,
                issue.Severity,
                issue.ColumnName ?? (object)DBNull.Value,
                issue.Message,
                issue.CreatedUtc);
        }

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(
                cancellationToken);

        try
        {
            using var bulkCopy =
                new SqlBulkCopy(
                    connection,
                    SqlBulkCopyOptions.CheckConstraints,
                    transaction)
                {
                    DestinationTableName =
                        "[migrations].[ValidationIssues]",

                    BatchSize = 1000,

                    BulkCopyTimeout = 0,

                    EnableStreaming = true
                };

            foreach (DataColumn column in table.Columns)
            {
                bulkCopy.ColumnMappings.Add(
                    column.ColumnName,
                    column.ColumnName);
            }

            await bulkCopy.WriteToServerAsync(
                table,
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }
}