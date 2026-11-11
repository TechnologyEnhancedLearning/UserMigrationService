
using System.Data;
using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace LearningHub.UserMigrationService.Services;

public sealed class AttributeMigrationService :
    IUserAttributeMigrationService
{
    private const int BatchSize = 5000;

    private readonly string _legacyConnectionString;
    private readonly string _learningHubConnectionString;

    public AttributeMigrationService(
        IOptions<DatabaseOptions> databaseOptions)
    {
        _legacyConnectionString =
            databaseOptions.Value.LegacyHubConnectionString;

        _learningHubConnectionString =
            databaseOptions.Value.LearningHubConnectionString;

        if (string.IsNullOrWhiteSpace(_legacyConnectionString))
        {
            throw new InvalidOperationException(
                "Legacy Hub connection string is missing.");
        }

        if (string.IsNullOrWhiteSpace(_learningHubConnectionString))
        {
            throw new InvalidOperationException(
                "Learning Hub connection string is missing.");
        }
    }


    // =========================================================
    // USER ATTRIBUTE MIGRATION
    // Extract ALL rows. No UserIdsToMigrate filtering.
    // =========================================================

    public async Task<MigrationStatistics>
        ExtractAndStageUserAttributesAsync(
            Guid migrationRunId,
            CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                userAttributeId,
                userId,
                attributeId,

                COALESCE(
                    textValue,
                    CONVERT(nvarchar(255), intValue),
                    CASE
                        WHEN booleanValue = 1 THEN N'true'
                        WHEN booleanValue = 0 THEN N'false'
                    END,
                    CONVERT(nvarchar(33), dateValue, 127)
                ) AS [Value],

                deleted,
                amendUserId,
                amendDate

            FROM dbo.userAttributeTBL where attributeId in (38,39)
            ORDER BY userAttributeId;
            """;

        var statistics = new MigrationStatistics();

        var batch = CreateUserAttributeDataTable();

        await using var sourceConnection =
            new SqlConnection(_legacyConnectionString);

        await using var destinationConnection =
            new SqlConnection(_learningHubConnectionString);

        await sourceConnection.OpenAsync(cancellationToken);
        await destinationConnection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, sourceConnection);

        command.CommandTimeout = 300;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        using var bulkCopy = new SqlBulkCopy(
            destinationConnection,
            SqlBulkCopyOptions.TableLock,
            null)
        {
            DestinationTableName = "[migrations].[UserAttribute]",
            BatchSize = BatchSize,
            BulkCopyTimeout = 300
        };

        AddUserAttributeColumnMappings(bulkCopy);

        while (await reader.ReadAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            statistics.RecordsRead++;

            var row = batch.NewRow();

            row["MigrationRunId"] = migrationRunId;

            row["LegacyUserAttributeId"] =
                reader.GetInt32(
                    reader.GetOrdinal("userAttributeId"));

            row["UserId"] =
                reader.GetInt32(
                    reader.GetOrdinal("userId"));

            row["AttributeId"] =
                reader.GetInt32(
                    reader.GetOrdinal("attributeId"));

            row["Value"] =
                GetNullableValue(reader, "Value");

            row["Deleted"] =
                reader.GetBoolean(
                    reader.GetOrdinal("deleted"));

            var amendUserId =
                reader.GetInt32(
                    reader.GetOrdinal("amendUserId"));

            var amendDate =
                reader.GetFieldValue<DateTimeOffset>(
                    reader.GetOrdinal("amendDate"));

            // Creation fields.
            row["CreateUserId"] = amendUserId;
            row["CreateDate"] = amendDate;

            // Amendment fields.
            row["AmendUserId"] = amendUserId;
            row["AmendDate"] = amendDate;

            batch.Rows.Add(row);

            if (batch.Rows.Count >= BatchSize)
            {
                await FlushBatchAsync(
                    bulkCopy,
                    batch,
                    cancellationToken);

                statistics.RecordsWritten += BatchSize;
            }
        }

        if (batch.Rows.Count > 0)
        {
            var count = batch.Rows.Count;

            await FlushBatchAsync(
                bulkCopy,
                batch,
                cancellationToken);

            statistics.RecordsWritten += count;
        }

        return statistics;
    }


    // =========================================================
    // USER ATTRIBUTE DATA TABLE
    // =========================================================

    private static DataTable CreateUserAttributeDataTable()
    {
        var table = new DataTable();

        table.Columns.Add("MigrationRunId", typeof(Guid));
        table.Columns.Add("LegacyUserAttributeId", typeof(int));
        table.Columns.Add("UserId", typeof(int));
        table.Columns.Add("AttributeId", typeof(int));
        table.Columns.Add("Value", typeof(string));
        table.Columns.Add("Deleted", typeof(bool));
        table.Columns.Add("CreateUserId", typeof(int));
        table.Columns.Add("CreateDate", typeof(DateTimeOffset));
        table.Columns.Add("AmendUserId", typeof(int));
        table.Columns.Add("AmendDate", typeof(DateTimeOffset));

        return table;
    }

    // =========================================================
    // USER ATTRIBUTE COLUMN MAPPINGS
    // =========================================================

    private static void AddUserAttributeColumnMappings(
        SqlBulkCopy bulkCopy)
    {
        var columns = new[]
        {
            "MigrationRunId",
            "LegacyUserAttributeId",
            "UserId",
            "AttributeId",
            "Value",
            "Deleted",
            "CreateUserId",
            "CreateDate",
            "AmendUserId",
            "AmendDate"
        };

        foreach (var column in columns)
        {
            bulkCopy.ColumnMappings.Add(column, column);
        }
    }

    // =========================================================
    // COMMON HELPERS
    // =========================================================

    private static object GetNullableValue(
        SqlDataReader reader,
        string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);

        return reader.IsDBNull(ordinal)
            ? DBNull.Value
            : reader.GetValue(ordinal);
    }

    private static async Task FlushBatchAsync(
        SqlBulkCopy bulkCopy,
        DataTable batch,
        CancellationToken cancellationToken)
    {
        await bulkCopy.WriteToServerAsync(
            batch,
            cancellationToken);

        batch.Clear();
    }
}
