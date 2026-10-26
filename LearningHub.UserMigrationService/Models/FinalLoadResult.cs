namespace LearningHub.UserMigrationService.Models;

public sealed class FinalLoadResult
{
    public long RecordsRead { get; init; }
    public long RecordsInserted { get; init; }
    public long RecordsUpdated { get; init; }
    public long RecordsSkipped { get; init; }
    public long RecordsFailed { get; init; }

    public MigrationStatistics ToMigrationStatistics() => new()
    {
        RecordsRead = RecordsRead,
        RecordsWritten = RecordsInserted + RecordsUpdated,
        RecordsSkipped = RecordsSkipped,
        RecordsFailed = RecordsFailed
    };
}
