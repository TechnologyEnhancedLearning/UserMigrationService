using LearningHub.UserMigrationService.Data;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningHub.UserMigrationService.Repositories;

public class MigrationRunRepository : IMigrationRunRepository
{
    private readonly UserMigrationDbContext _context;

    public MigrationRunRepository(UserMigrationDbContext context)
    {
        _context = context;
    }

    public async Task InsertMigrationRunAsync(MigrationRun migrationRun)
    {
        _context.MigrationRuns.Add(migrationRun);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateMigrationRunAsync(MigrationRun migrationRun)
    {
        var existingRun = await _context.MigrationRuns
            .FirstOrDefaultAsync(x => x.MigrationRunId == migrationRun.MigrationRunId);

        if (existingRun == null)
        {
            throw new InvalidOperationException(
                $"Migration run '{migrationRun.MigrationRunId}' was not found.");
        }

        existingRun.CompletedUtc = migrationRun.CompletedUtc;
        existingRun.Status = migrationRun.Status;
        existingRun.TotalDurationMs = migrationRun.TotalDurationMs;
        existingRun.TotalSteps = migrationRun.TotalSteps;
        existingRun.SuccessfulSteps = migrationRun.SuccessfulSteps;
        existingRun.FailedSteps = migrationRun.FailedSteps;
        existingRun.TotalRecordsRead = migrationRun.TotalRecordsRead;
        existingRun.TotalRecordsWritten = migrationRun.TotalRecordsWritten;
        existingRun.TotalRecordsSkipped = migrationRun.TotalRecordsSkipped;
        existingRun.TotalRecordsFailed = migrationRun.TotalRecordsFailed;
        existingRun.ErrorMessage = migrationRun.ErrorMessage;

        await _context.SaveChangesAsync();
    }

    public async Task InsertMigrationStepRunAsync(MigrationStepRun stepRun)
    {
        _context.MigrationStepRuns.Add(stepRun);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateMigrationStepRunAsync(
     MigrationStepRun stepRun,
     MigrationStatistics statistics,
     string? errorMessage = null)
    {
        var existing = await _context.MigrationStepRuns
            .FirstOrDefaultAsync(x => x.MigrationStepRunId == stepRun.MigrationStepRunId);

        if (existing == null)
        {
            throw new InvalidOperationException(
                $"Migration step '{stepRun.MigrationStepRunId}' not found.");
        }

        existing.CompletedUtc = stepRun.CompletedUtc;
        existing.Status = stepRun.Status;

        existing.DurationMs =
            existing.CompletedUtc.HasValue
                ? (long)(existing.CompletedUtc.Value - existing.StartedUtc).TotalMilliseconds
                : null;

        existing.RecordsRead = statistics.RecordsRead;
        existing.RecordsWritten = statistics.RecordsWritten;
        existing.RecordsSkipped = statistics.RecordsSkipped;
        existing.RecordsFailed = statistics.RecordsFailed;

        existing.ErrorMessage = errorMessage;

        await _context.SaveChangesAsync();
    }

    public async Task InsertMigrationLogAsync(MigrationLog log)
    {
        _context.MigrationLogs.Add(log);

        await _context.SaveChangesAsync();
    }
}