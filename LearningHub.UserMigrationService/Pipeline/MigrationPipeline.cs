using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models;
using LearningHub.UserMigrationService.Models.Transformation;
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
    private readonly IUserExtractor _userExtractor;
    private readonly IUserTransformer _userTransformer;
    private readonly IUserEmploymentExtractor _userEmploymentExtractor;
    private readonly IUserEmploymentTransformer _userEmploymentTransformer;
    private readonly IUserAdminLocationExtractor _userAdminLocationExtractor;
    private readonly IUserAdminLocationTransformer _userAdminLocationTransformer;
    private readonly IUserGroupReporterExtractor _userGroupReporterExtractor;
    private readonly IUserGroupReporterTransformer _userGroupReporterTransformer;
    private readonly IOrganisationExtractor _organisationExtractor;
    private readonly ISupportingLookupExtractor _supportingLookupExtractor;
    private readonly IOrganisationTransformer _organisationTransformer;
    private readonly IOrganisationTypeTransformer _organisationTypeTransformer;
    private readonly IOrganisationTypeMappingRepository _organisationTypeMappingRepository;

    public MigrationPipeline(
        ILearningHubRepository learningHubRepository,
        ILegacyRepository legacyRepository,
        IMigrationLogger migrationLogger,
        IUserMigrationSelectionService userMigrationSelectionService,
        IOrganisationMigrationSelectionService organisationMigrationSelectionService,
        IProfessionalBodyExtractor professionalBodyExtractor,
        IProfessionalBodyMappingRepository professionalBodyMappingRepository,
        IProfessionalBodyTransformer professionalBodyTransformer,
        IStagingRepository stagingRepository,
        IUserExtractor userExtractor,
        IUserTransformer userTransformer,
        IUserEmploymentExtractor userEmploymentExtractor,
        IUserEmploymentTransformer userEmploymentTransformer,
        IUserAdminLocationExtractor userAdminLocationExtractor,
        IUserAdminLocationTransformer userAdminLocationTransformer,
        IUserGroupReporterExtractor userGroupReporterExtractor,
        IUserGroupReporterTransformer userGroupReporterTransformer,
        IOrganisationExtractor organisationExtractor,
        ISupportingLookupExtractor supportingLookupExtractor,
        IOrganisationTransformer organisationTransformer,
        IOrganisationTypeTransformer organisationTypeTransformer,
        IOrganisationTypeMappingRepository organisationTypeMappingRepository)
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
        _userExtractor = userExtractor;
        _userTransformer = userTransformer;
        _userEmploymentExtractor = userEmploymentExtractor;
        _userEmploymentTransformer = userEmploymentTransformer;
        _userAdminLocationExtractor = userAdminLocationExtractor;
        _userAdminLocationTransformer = userAdminLocationTransformer;
        _userGroupReporterExtractor = userGroupReporterExtractor;
        _userGroupReporterTransformer = userGroupReporterTransformer;
        _organisationExtractor = organisationExtractor;
        _supportingLookupExtractor = supportingLookupExtractor;
        _organisationTransformer = organisationTransformer;
        _organisationTypeTransformer = organisationTypeTransformer;
        _organisationTypeMappingRepository = organisationTypeMappingRepository;
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

                var professionalBodyStatistics = await TransformAndStageProfessionalBodiesAsync(
                     migrationRunId,
                     cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    professionalBodyStepId,
                    professionalBodyStatistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    professionalBodyStepId,
                    "Information",
                    "ProfessionalBodyTransformer",
                    "Professional Body extraction, transformation and staging completed.");

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
            // Step 6 - Transform and stage Users
            // ==================================================

            var usersStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Transform and Stage Users");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    usersStepId,
                    "Information",
                    "UserTransformer",
                    "Starting User extraction, transformation and staging.");

                var userStatistics =
                    await TransformAndStageUsersAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    usersStepId,
                    userStatistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    usersStepId,
                    "Information",
                    "UserTransformer",
                    "User extraction, transformation and staging completed.");

                Console.WriteLine(
                    "User transformation and staging completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    usersStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    usersStepId,
                    "Error",
                    "UserTransformer",
                    "User transformation and staging failed.",
                    ex);

                throw;
            }

            // ==================================================
            // Step 7 - Transform and stage User Employments
            // ==================================================

            var userEmploymentStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Transform and Stage User Employments");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userEmploymentStepId,
                    "Information",
                    "UserEmploymentTransformer",
                    "Starting User Employment extraction, transformation and staging.");

                var userEmploymentStatistics =
                    await TransformAndStageUserEmploymentAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    userEmploymentStepId,
                    userEmploymentStatistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userEmploymentStepId,
                    "Information",
                    "UserEmploymentTransformer",
                    "User Employment extraction, transformation and staging completed.");

                Console.WriteLine(
                    "User Employment transformation and staging completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    userEmploymentStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userEmploymentStepId,
                    "Error",
                    "UserEmploymentTransformer",
                    "User Employment transformation and staging failed.",
                    ex);

                throw;
            }

            // ==================================================
            // Step 8 - Transform and stage User Admin Locations
            // ==================================================

            var userAdminLocationStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Transform and Stage User Admin Locations");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userAdminLocationStepId,
                    "Information",
                    "UserAdminLocationTransformer",
                    "Starting User Admin Location extraction, transformation and staging.");

                var userAdminLocationStatistics =
                    await TransformAndStageUserAdminLocationsAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    userAdminLocationStepId,
                    userAdminLocationStatistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userAdminLocationStepId,
                    "Information",
                    "UserAdminLocationTransformer",
                    "User Admin Location extraction, transformation and staging completed.");

                Console.WriteLine(
                    "User Admin Location transformation and staging completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    userAdminLocationStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userAdminLocationStepId,
                    "Error",
                    "UserAdminLocationTransformer",
                    "User Admin Location transformation and staging failed.",
                    ex);

                throw;
            }

            // ==================================================
            // Step 9 - Transform and stage User Group Reporters
            // ==================================================

            var userGroupReporterStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Transform and Stage User Group Reporters");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userGroupReporterStepId,
                    "Information",
                    "UserGroupReporterTransformer",
                    "Starting User Group Reporter extraction, transformation and staging.");

                var userGroupReporterStatistics =
                    await TransformAndStageUserGroupReportersAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    userGroupReporterStepId,
                    userGroupReporterStatistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userGroupReporterStepId,
                    "Information",
                    "UserGroupReporterTransformer",
                    "User Group Reporter extraction, transformation and staging completed.");

                Console.WriteLine(
                    "User Group Reporter transformation and staging completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    userGroupReporterStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userGroupReporterStepId,
                    "Error",
                    "UserGroupReporterTransformer",
                    "User Group Reporter transformation and staging failed.",
                    ex);

                throw;
            }
            // ==================================================
            // Step 10 - Transform and stage Organisation Types
            // ==================================================

            var organisationTypeStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Transform and Stage Organisation Types");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    organisationTypeStepId,
                    "Information",
                    "OrganisationTypeTransformer",
                    "Starting Organisation Type extraction, transformation and staging.");

                var organisationTypeStatistics =
                    await TransformAndStageOrganisationTypesAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    organisationTypeStepId,
                    organisationTypeStatistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    organisationTypeStepId,
                    "Information",
                    "OrganisationTypeTransformer",
                    "Organisation Type extraction, transformation and staging completed.");

                Console.WriteLine(
                    "Organisation Type transformation and staging completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    organisationTypeStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    organisationTypeStepId,
                    "Error",
                    "OrganisationTypeTransformer",
                    "Organisation Type transformation and staging failed.",
                    ex);

                throw;
            }
            // ==================================================
            // Step 11 - Transform and stage Organisations
            // ==================================================

            var organisationStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Transform and Stage Organisations");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    organisationStepId,
                    "Information",
                    "OrganisationTransformer",
                    "Starting Organisation extraction, transformation and staging.");

                var organisationStatistics =
                    await TransformAndStageOrganisationsAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    organisationStepId,
                    organisationStatistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    organisationStepId,
                    "Information",
                    "OrganisationTransformer",
                    "Organisation extraction, transformation and staging completed.");

                Console.WriteLine(
                    "Organisation transformation and staging completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    organisationStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    organisationStepId,
                    "Error",
                    "OrganisationTransformer",
                    "Organisation transformation and staging failed.",
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
    private async Task<MigrationStatistics>
    TransformAndStageProfessionalBodiesAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics = new MigrationStatistics();

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
            statistics.RecordsRead++;

            mappingDictionary.TryGetValue(
                source.ProfessionalBodyId,
                out var mapping);

            var transformed =
                _professionalBodyTransformer.Transform(
                    source,
                    mapping,
                    migrationRunId);

            var validationErrors =
                TransformationValidator.Validate(
                    transformed);

            if (validationErrors.Count > 0)
            {
                statistics.RecordsFailed++;

                foreach (var error in validationErrors)
                {
                    await _migrationLogger.LogAsync(
                        migrationRunId,
                        null,
                        "Warning",
                        "ProfessionalBodyTransformer",
                        $"Legacy Professional Body " +
                        $"{source.ProfessionalBodyId}: {error}");
                }

                continue;
            }

            await _stagingRepository
                .InsertProfessionalBodyAsync(
                    transformed,
                    cancellationToken);

            statistics.RecordsWritten++;
        }

        return statistics;
    }
    private async Task<MigrationStatistics>
    TransformAndStageUsersAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics = new MigrationStatistics();

        var userIds =
            await _learningHubRepository
                .GetUserIdsToMigrateAsync(
                    cancellationToken);

        var users =
            _userExtractor.ExtractAsync(
                userIds,
                cancellationToken);

        await foreach (
            var source in users
                .WithCancellation(cancellationToken))
        {
            statistics.RecordsRead++;

            var transformed =
                _userTransformer.Transform(
                    source,
                    migrationRunId);

            var validationErrors =
                TransformationValidator.Validate(
                    transformed);

            if (validationErrors.Count > 0)
            {
                statistics.RecordsFailed++;

                foreach (var error in validationErrors)
                {
                    await _migrationLogger.LogAsync(
                        migrationRunId,
                        null,
                        "Warning",
                        "UserTransformer",
                        $"Legacy User " +
                        $"{source.UserId}: {error}");
                }

                continue;
            }

            await _stagingRepository
                .InsertUserAsync(
                    transformed,
                    cancellationToken);

            statistics.RecordsWritten++;

            if (transformed.IsRemoved)
            {
                statistics.RecordsRemoved++;
            }
        }

        return statistics;
    }
    private async Task<MigrationStatistics>
    TransformAndStageUserEmploymentAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics = new MigrationStatistics();

        var userIds =
            await _learningHubRepository
                .GetUserIdsToMigrateAsync(
                    cancellationToken);

        var employmentRecords =
            _userEmploymentExtractor.ExtractAsync(
                userIds,
                cancellationToken);

        await foreach (
            var source in employmentRecords
                .WithCancellation(cancellationToken))
        {
            statistics.RecordsRead++;

            var transformed =
                _userEmploymentTransformer.Transform(
                    source,
                    migrationRunId);

            await _stagingRepository
                .InsertUserEmploymentAsync(
                    transformed,
                    cancellationToken);

            statistics.RecordsWritten++;

            if (transformed.IsRemoved)
            {
                statistics.RecordsRemoved++;
            }
        }

        return statistics;
    }
    private async Task<MigrationStatistics>
    TransformAndStageUserAdminLocationsAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics = new MigrationStatistics();

        var userIds =
            await _learningHubRepository
                .GetUserIdsToMigrateAsync(
                    cancellationToken);

        var records =
            _userAdminLocationExtractor.ExtractAsync(
                userIds,
                cancellationToken);

        await foreach (
            var source in records
                .WithCancellation(cancellationToken))
        {
            statistics.RecordsRead++;

            var transformed =
                _userAdminLocationTransformer.Transform(
                    source,
                    migrationRunId);

            await _stagingRepository
                .InsertUserAdminLocationAsync(
                    transformed,
                    cancellationToken);

            statistics.RecordsWritten++;

            if (transformed.IsRemoved)
            {
                statistics.RecordsRemoved++;
            }
        }

        return statistics;
    }
    private async Task<MigrationStatistics>
    TransformAndStageUserGroupReportersAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics = new MigrationStatistics();

        var userIds =
            await _learningHubRepository
                .GetUserIdsToMigrateAsync(
                    cancellationToken);

        var records =
            _userGroupReporterExtractor.ExtractAsync(
                userIds,
                cancellationToken);

        await foreach (
            var source in records
                .WithCancellation(cancellationToken))
        {
            statistics.RecordsRead++;

            var transformed =
                _userGroupReporterTransformer.Transform(
                    source,
                    migrationRunId);

            await _stagingRepository
                .InsertUserGroupReporterAsync(
                    transformed,
                    cancellationToken);

            statistics.RecordsWritten++;

            if (transformed.IsRemoved)
            {
                statistics.RecordsRemoved++;
            }
        }

        return statistics;
    }
    private async Task<MigrationStatistics>
    TransformAndStageOrganisationTypesAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics =
            new MigrationStatistics();

        var mappings =
            await _organisationTypeMappingRepository
                .GetMappingsAsync(
                    cancellationToken);

        var mappingDictionary =
            mappings.ToDictionary(
                x => x.LegacyOrganisationTypeId);

        var organisationTypes =
            _supportingLookupExtractor
                .ExtractOrganisationTypesAsync(
                    cancellationToken);

        await foreach (
            var source in organisationTypes
                .WithCancellation(cancellationToken))
        {
            statistics.RecordsRead++;

            mappingDictionary.TryGetValue(
                source.Id,
                out var mapping);

            var transformed =
                _organisationTypeTransformer.Transform(
                    source,
                    mapping?.OrganisationTypeId,
                    mapping?.OrganisationType,
                    mapping?.IsMapped ?? false,
                    migrationRunId);

            var validationErrors =
                TransformationValidator.Validate(
                    transformed);

            if (validationErrors.Count > 0)
            {
                statistics.RecordsFailed++;

                foreach (var error in validationErrors)
                {
                    await _migrationLogger.LogAsync(
                        migrationRunId,
                        null,
                        "Warning",
                        "OrganisationTypeTransformer",
                        $"Legacy Organisation Type " +
                        $"{source.Id}: {error}");
                }

                continue;
            }

            await _stagingRepository
                .InsertOrganisationTypeAsync(
                    transformed,
                    cancellationToken);

            statistics.RecordsWritten++;

            if (transformed.IsRemoved)
            {
                statistics.RecordsRemoved++;
            }
        }

        return statistics;
    }
    private async Task<MigrationStatistics>
    TransformAndStageOrganisationsAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics =
            new MigrationStatistics();

        var locationIds =
            await _learningHubRepository
                .GetOrganisationLocationIdsToMigrateAsync(
                    cancellationToken);

        var mappings =
            await _organisationTypeMappingRepository
                .GetMappingsAsync(
                    cancellationToken);

        var mappingDictionary =
            mappings.ToDictionary(
                x => x.LegacyOrganisationTypeId);

        var organisations =
            _organisationExtractor.ExtractAsync(
                locationIds,
                cancellationToken);

        await foreach (
            var source in organisations
                .WithCancellation(cancellationToken))
        {
            statistics.RecordsRead++;

            OrganisationTypeMapping? mapping = null;

            if (source.LocationTypeId.HasValue)
            {
                mappingDictionary.TryGetValue(
                    source.LocationTypeId.Value,
                    out mapping);
            }

            var transformed =
                _organisationTransformer.Transform(
                    source,
                    mapping,
                    migrationRunId);

            var validationErrors =
                TransformationValidator.Validate(
                    transformed);

            if (validationErrors.Count > 0)
            {
                statistics.RecordsFailed++;

                foreach (var error in validationErrors)
                {
                    await _migrationLogger.LogAsync(
                        migrationRunId,
                        null,
                        "Warning",
                        "OrganisationTransformer",
                        $"Legacy Organisation " +
                        $"{source.LocationId}: {error}");
                }

                continue;
            }

            await _stagingRepository
                .InsertOrganisationAsync(
                    transformed,
                    cancellationToken);

            statistics.RecordsWritten++;

            if (transformed.IsRemoved)
            {
                statistics.RecordsRemoved++;
            }
        }

        return statistics;
    }
}