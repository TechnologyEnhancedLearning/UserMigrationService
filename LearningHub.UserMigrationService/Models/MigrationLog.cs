public class MigrationLog
{
    public long MigrationLogId { get; set; }

    public Guid MigrationRunId { get; set; }

    public Guid? MigrationStepRunId { get; set; }

    public string LogLevel { get; set; } = string.Empty;

    public string Component { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? Exception { get; set; }

    public DateTime CreatedUtc { get; set; }
}