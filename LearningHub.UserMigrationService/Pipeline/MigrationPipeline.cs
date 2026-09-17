using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models;
using LearningHub.UserMigrationService.Services;


namespace LearningHub.UserMigrationService.Pipeline;

public class MigrationPipeline : IMigrationPipeline
{
    private readonly ILearningHubRepository _learningHubRepository;
    private readonly ILegacyRepository _legacyRepository;
    private readonly IMigrationLogger _migrationLogger;
    private readonly IUserMigrationSelectionService _userMigrationSelectionService;
    private readonly IOrganisationMigrationSelectionService _organisationMigrationSelectionService;
    private readonly IProfessionalBodyExtractor _professionalBodyExtractor;
    private readonly IProfessionalBodyMappingRepository _professionalBodyMappingRepository;
    private readonly IProfessionalBodyTransformer _professionalBodyTransformer;
    private readonly IStagingRepository _stagingRepository;

    public MigrationPipeline(
        ILearningHubRepository learningHubRepository,
        ILegacyRepository legacyRepository,
        IMigrationLogger migrationLogger,
        IUserMigrationSelectionService userMigrationSelectionService,
        IOrganisationMigrationSelectionService organisationMigrationSelectionService,
        IProfessionalBodyExtractor professionalBodyExtractor,
        IProfessionalBodyMappingRepository professionalBodyMappingRepository,
        IProfessionalBodyTransformer professionalBodyTransformer,
        IStagingRepository stagingRepository)
    {
        _learningHubRepository = learningHubRepository;
        _legacyRepository = legacyRepository;
        _migrationLogger = migrationLogger;
        _userMigrationSelectionService = userMigrationSelectionService;
        _organisationMigrationSelectionService = organisationMigrationSelectionService;
        _professionalBodyExtractor = professionalBodyExtractor;
        _professionalBodyMappingRepository = professionalBodyMappingRepository;
        _professionalBodyTransformer = professionalBodyTransformer;
        _stagingRepository = stagingRepository;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("MigrationPipeline started.");

        Guid migrationRunId =
            await _migrationLogger.StartMigrationAsync(
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

            // ==================================================
            // Step 1 - Test Learning Hub connection
            // ==================================================

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

            // ==================================================
            // Step 2 - Test eLFH connection
            // ==================================================

            var legacyStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Test eLFH Connection");

            await _migrationLogger.LogAsync(
                migrationRunId,
                legacyStepId,
                "Information",
                "eLFHRepository",
                "eLFH connection test started.");

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
                    "eLFH connection test completed.");

                Console.WriteLine(
                    "eLFH connection test completed.");
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
                    "eLFH connection test failed.",
                    ex);

                throw;
            }

            // ==================================================
            // Step 3 - Identify users to migrate
            // ==================================================

            var identifyUsersStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Identify Users To Migrate");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    identifyUsersStepId,
                    "Information",
                    "UserMigrationSelectionService",
                    "Identifying users eligible for migration.");

                var usersWritten =
                    await _userMigrationSelectionService
                        .PopulateUsersToMigrateAsync(
                            cancellationToken);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    identifyUsersStepId,
                    "Information",
                    "UserMigrationSelectionService",
                    $"Populated migrations.UserIdsToMigrate with {usersWritten} users.");

                await _migrationLogger.CompleteStepAsync(
                    identifyUsersStepId,
                    new MigrationStatistics
                    {
                        RecordsWritten = usersWritten
                    });

                Console.WriteLine(
                    $"User migration selection completed. " +
                    $"Users selected: {usersWritten}");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    identifyUsersStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    identifyUsersStepId,
                    "Error",
                    "UserMigrationSelectionService",
                    "Failed to identify users for migration.",
                    ex);

                throw;
            }
            
            // ==================================================
            // Step 4 - Identify organisations to migrate
            // ==================================================

            var identifyOrganisationsStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Identify Organisations To Migrate");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    identifyOrganisationsStepId,
                    "Information",
                    "OrganisationMigrationSelectionService",
                    "Identifying organisation locations for migration.");

                // Get the users selected during Step 3.
                var userIds =
                    await _learningHubRepository
                        .GetUserIdsToMigrateAsync(
                            cancellationToken);

                var organisationsWritten =
                    await _organisationMigrationSelectionService
                        .PopulateOrganisationLocationsToMigrateAsync(
                            userIds,
                            cancellationToken);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    identifyOrganisationsStepId,
                    "Information",
                    "OrganisationMigrationSelectionService",
                    $"Populated migrations.OrganisationLocationIdsToMigrate " +
                    $"with {organisationsWritten} organisation locations.");

                await _migrationLogger.CompleteStepAsync(
                    identifyOrganisationsStepId,
                    new MigrationStatistics
                    {
                        RecordsWritten = organisationsWritten
                    });

                Console.WriteLine(
                    $"Organisation migration selection completed. " +
                    $"Locations selected: {organisationsWritten}");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    identifyOrganisationsStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    identifyOrganisationsStepId,
                    "Error",
                    "OrganisationMigrationSelectionService",
                    "Failed to identify organisations for migration.",
                    ex);

                throw;
            }

            // ==================================================
            // Step 5 - Transform and stage Professional Bodies
            // ==================================================

            var professionalBodyStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Transform and Stage Professional Bodies");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    professionalBodyStepId,
                    "Information",
                    "ProfessionalBodyTransformer",
                    "Starting Professional Body extraction, transformation and staging.");

                await TransformAndStageProfessionalBodiesAsync(
                    migrationRunId,
                    cancellationToken);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    professionalBodyStepId,
                    "Information",
                    "ProfessionalBodyTransformer",
                    "Professional Body extraction, transformation and staging completed.");

                await _migrationLogger.CompleteStepAsync(
                    professionalBodyStepId,
                    new MigrationStatistics());

                Console.WriteLine(
                    "Professional Body transformation and staging completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    professionalBodyStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    professionalBodyStepId,
                    "Error",
                    "ProfessionalBodyTransformer",
                    "Professional Body transformation and staging failed.",
                    ex);

                throw;
            }

            // ==================================================
            // Migration completed
            // ==================================================

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
    private async Task TransformAndStageProfessionalBodiesAsync( Guid migrationRunId, CancellationToken cancellationToken)
    {
        var professionalBodies =
            _professionalBodyExtractor
                .ExtractAsync(cancellationToken);

        var mappings =
            await _professionalBodyMappingRepository
                .GetMappingsAsync(cancellationToken);

        var mappingDictionary =
            mappings.ToDictionary(
                x => x.LegacyProfessionalBodyId);

        await foreach (
            var source in professionalBodies
                .WithCancellation(cancellationToken))
        {
            mappingDictionary.TryGetValue(
                source.ProfessionalBodyId,
                out var mapping);

            var transformed =
                _professionalBodyTransformer.Transform(
                    source,
                    mapping,
                    migrationRunId);

            await _stagingRepository
                .InsertProfessionalBodyAsync(
                    transformed,
                    cancellationToken);
        }
    }
}