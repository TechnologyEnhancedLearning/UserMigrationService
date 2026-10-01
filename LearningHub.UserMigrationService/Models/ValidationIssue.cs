namespace LearningHub.UserMigrationService.Models;

public class ValidationIssue
{
    public long Id { get; set; }

    public Guid MigrationRunId { get; set; }

    public string TableName { get; set; } = string.Empty;

    public long? StagingRecordId { get; set; }

    public int? ElfhRecordId { get; set; }

    public string ValidationType { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string? ColumnName { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedUtc { get; set; }
}