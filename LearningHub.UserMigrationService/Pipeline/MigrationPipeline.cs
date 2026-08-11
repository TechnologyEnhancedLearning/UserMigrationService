using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Models;

namespace LearningHub.UserMigrationService.Pipeline;

public class MigrationPipeline : IMigrationPipeline
{
    private readonly ILearningHubRepository _learningHubRepository;
    private readonly ILegacyRepository _legacyRepository;
    private readonly IMigrationLogger _migrationLogger;

    public MigrationPipeline(
        ILearningHubRepository learningHubRepository,
        ILegacyRepository legacyRepository,
        IMigrationLogger migrationLogger)
    {
        _learningHubRepository = learningHubRepository;
        _legacyRepository = legacyRepository;
        _migrationLogger = migrationLogger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("MigrationPipeline started.");

        Guid migrationRunId = await _migrationLogger.StartMigrationAsync(
            "User Migration",
            "LocalDevelopment",
            "1.0.0");

        try
        {
            await _migrationLogger.LogAsync(
                migrationRunId,
                null,
                "Information",
                "MigrationPipeline",
                "Migration started.");

            // --------------------------------------------------
            // Step 1 - Test Learning Hub connection
            // --------------------------------------------------

            var learningHubStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Test Learning Hub Connection");

            await _migrationLogger.LogAsync(
                migrationRunId,
                learningHubStepId,
                "Information",
                "LearningHubRepository",
                "Learning Hub connection test started.");

            try
            {
                await _learningHubRepository.TestConnectionAsync();

                await _migrationLogger.CompleteStepAsync(
                    learningHubStepId,
                    new MigrationStatistics());

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    learningHubStepId,
                    "Information",
                    "LearningHubRepository",
                    "Learning Hub connection test completed.");

                Console.WriteLine(
                    "Learning Hub connection test completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    learningHubStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    learningHubStepId,
                    "Error",
                    "LearningHubRepository",
                    "Learning Hub connection test failed.",
                    ex);

                throw;
            }

            // --------------------------------------------------
            // Step 2 - Test Legacy eLFH connection
            // --------------------------------------------------

            var legacyStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Test Legacy eLFH Connection");

            await _migrationLogger.LogAsync(
                migrationRunId,
                legacyStepId,
                "Information",
                "LegacyRepository",
                "Legacy eLFH connection test started.");

            try
            {
                await _legacyRepository.TestConnectionAsync();

                await _migrationLogger.CompleteStepAsync(
                    legacyStepId,
                    new MigrationStatistics());

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    legacyStepId,
                    "Information",
                    "LegacyRepository",
                    "Legacy eLFH connection test completed.");

                Console.WriteLine(
                    "Legacy eLFH connection test completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    legacyStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    legacyStepId,
                    "Error",
                    "LegacyRepository",
                    "Legacy eLFH connection test failed.",
                    ex);

                throw;
            }

            // --------------------------------------------------
            // Migration completed
            // --------------------------------------------------

            await _migrationLogger.CompleteMigrationAsync(
                migrationRunId);

            await _migrationLogger.LogAsync(
                migrationRunId,
                null,
                "Information",
                "MigrationPipeline",
                "Migration completed.");

            Console.WriteLine(
                "MigrationPipeline completed.");
        }
        catch (Exception ex)
        {
            await _migrationLogger.FailMigrationAsync(
                migrationRunId,
                ex);

            await _migrationLogger.LogAsync(
                migrationRunId,
                null,
                "Error",
                "MigrationPipeline",
                "Migration failed.",
                ex);

            Console.WriteLine(
                $"MigrationPipeline failed: {ex.Message}");

            throw;
        }
    }
}