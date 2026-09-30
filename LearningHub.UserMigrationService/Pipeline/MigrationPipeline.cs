using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models;
using LearningHub.UserMigrationService.Models.Transformation;
using LearningHub.UserMigrationService.Services;
using LearningHub.UserMigrationService.Transformers;
using Microsoft.Extensions.Options;

namespace LearningHub.UserMigrationService.Pipeline;

public class MigrationPipeline : IMigrationPipeline
{
    private readonly ILearningHubRepository _learningHubRepository;
    private readonly ILegacyRepository _legacyRepository;
    private readonly IMigrationLogger _migrationLogger;
    private readonly IUserMigrationSelectionService _userMigrationSelectionService;
    private readonly IOrganisationMigrationSelectionService _organisationMigrationSelectionService;


    private readonly IProfessionalBodyExtractor _professionalBodyExtractor;
    private readonly IUserExtractor _userExtractor;
    private readonly IUserAdminLocationExtractor _userAdminLocationExtractor;
    private readonly IUserAdminLocationTransformer _userAdminLocationTransformer;
    private readonly IUserGroupReporterExtractor _userGroupReporterExtractor;
    private readonly IUserGroupReporterTransformer _userGroupReporterTransformer;
    private readonly ISupportingLookupExtractor _supportingLookupExtractor;
    private readonly IUserEmploymentExtractor _userEmploymentExtractor;
    private readonly IOrganisationExtractor _organisationExtractor;

    private readonly IOrganisationTransformer _organisationTransformer;
    private readonly IOrganisationTypeTransformer _organisationTypeTransformer;
    private readonly IUserTransformer _userTransformer;
    private readonly IUserEmploymentTransformer _userEmploymentTransformer;
    private readonly IProfessionalBodyTransformer _professionalBodyTransformer;
    private readonly IUserOrganisationTransformer _userOrganisationTransformer;
    private readonly IUserRoleTransformer  _userRoleTransformer;
    private readonly IUserGroupRoleTransformer  _userGroupRoleTransformer;


    private readonly IProfessionalBodyMappingRepository _professionalBodyMappingRepository;
    private readonly IStagingRepository _stagingRepository;
    private readonly IOrganisationTypeMappingRepository _organisationTypeMappingRepository;
    private readonly IStaffGroupJobRoleTypeMappingRepository _staffGroupJobRoleTypeMappingRepository;
    private readonly int _batchSize;

    public MigrationPipeline(

        IProfessionalBodyExtractor professionalBodyExtractor,
        IUserExtractor userExtractor,
        IUserEmploymentExtractor userEmploymentExtractor,
        IUserAdminLocationExtractor userAdminLocationExtractor,
        IUserGroupReporterExtractor userGroupReporterExtractor,
        IOrganisationExtractor organisationExtractor,
        ISupportingLookupExtractor supportingLookupExtractor,

        IProfessionalBodyTransformer professionalBodyTransformer,
        IUserTransformer userTransformer,
        IUserEmploymentTransformer userEmploymentTransformer,
        IUserAdminLocationTransformer userAdminLocationTransformer,
        IUserGroupReporterTransformer userGroupReporterTransformer,
        IOrganisationTransformer organisationTransformer,
        IOrganisationTypeTransformer organisationTypeTransformer,
        IUserOrganisationTransformer userOrganisationTransformer,
        IUserRoleTransformer userRoleTransformer,
        IUserGroupRoleTransformer userGroupRoleTransformer,

        IMigrationLogger migrationLogger,
        IUserMigrationSelectionService userMigrationSelectionService,
        IOrganisationMigrationSelectionService organisationMigrationSelectionService,

        ILegacyRepository legacyRepository,
        ILearningHubRepository learningHubRepository,
        IOrganisationTypeMappingRepository organisationTypeMappingRepository,
        IProfessionalBodyMappingRepository professionalBodyMappingRepository,
        IStagingRepository stagingRepository,
        IStaffGroupJobRoleTypeMappingRepository staffGroupJobRoleTypeMappingRepository,
        IOptions<MigrationOptions> migrationOptions)
    {
        
       
        _professionalBodyExtractor = professionalBodyExtractor;
        _userExtractor = userExtractor;
        _userEmploymentExtractor = userEmploymentExtractor;
        _userAdminLocationExtractor = userAdminLocationExtractor;
        _userGroupReporterExtractor = userGroupReporterExtractor;
        _organisationExtractor = organisationExtractor;
        _supportingLookupExtractor = supportingLookupExtractor;

        _userTransformer = userTransformer;
        _organisationTransformer = organisationTransformer;
        _organisationTypeTransformer = organisationTypeTransformer;
        _userGroupReporterTransformer = userGroupReporterTransformer;
        _userEmploymentTransformer = userEmploymentTransformer;
        _userAdminLocationTransformer = userAdminLocationTransformer;
        _professionalBodyTransformer = professionalBodyTransformer;
        _userOrganisationTransformer = userOrganisationTransformer;
        _userRoleTransformer = userRoleTransformer;
        _userGroupRoleTransformer = userGroupRoleTransformer;

        _migrationLogger = migrationLogger;
        _userMigrationSelectionService = userMigrationSelectionService;
        _organisationMigrationSelectionService = organisationMigrationSelectionService;

        _organisationTypeMappingRepository = organisationTypeMappingRepository;
        _learningHubRepository = learningHubRepository;
        _legacyRepository = legacyRepository;
        _professionalBodyMappingRepository = professionalBodyMappingRepository;
        _stagingRepository = stagingRepository;
        _staffGroupJobRoleTypeMappingRepository = staffGroupJobRoleTypeMappingRepository;

        _batchSize = migrationOptions.Value.BatchSize;
        if (_batchSize <= 0)
        {
            throw new InvalidOperationException(
                "MigrationOptions:BatchSize must be greater than zero.");
        }
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
            // Step 10 - Transform and stage User Organisations
            // ==================================================

            var userOrganisationStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Transform and Stage User Organisations");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userOrganisationStepId,
                    "Information",
                    "UserOrganisationTransformer",
                    "Starting User Organisation extraction, transformation and staging.");

                var statistics =
                    await TransformAndStageUserOrganisationsAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    userOrganisationStepId,
                    statistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userOrganisationStepId,
                    "Information",
                    "UserOrganisationTransformer",
                    "User Organisation transformation and staging completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    userOrganisationStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    userOrganisationStepId,
                    "Error",
                    "UserOrganisationTransformer",
                    "User Organisation transformation and staging failed.",
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
        var transformedRecords =
   new List<TransformedProfessionalBody>();
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

            transformedRecords.Add(transformed);

            if (transformedRecords.Count >= _batchSize)
            {
                await _stagingRepository.InsertProfessionalBodiesAsync(
                    transformedRecords,
                    cancellationToken);

                statistics.RecordsWritten +=
                    transformedRecords.Count;

                transformedRecords.Clear();
            }
        }
        if (transformedRecords.Count > 0)
        {
            await _stagingRepository.InsertProfessionalBodiesAsync(
                transformedRecords,
                cancellationToken);

            statistics.RecordsWritten +=
                transformedRecords.Count;
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

        foreach (var userBatch in userIds.Chunk(_batchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var transformedUsers =
                new List<TransformedUser>(_batchSize);

            var users =
                _userExtractor.ExtractAsync(
                    userBatch,
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
                            $"Legacy User {source.UserId}: {error}");
                    }

                    continue;
                }

                transformedUsers.Add(transformed);

                if (transformed.RemoveDate.HasValue)
                {
                    statistics.RecordsRemoved++;
                }
            }

            if (transformedUsers.Count > 0)
            {
                await _stagingRepository.InsertUsersAsync(
                    transformedUsers,
                    cancellationToken);

                statistics.RecordsWritten +=
                    transformedUsers.Count;
            }

            Console.WriteLine(
                $"Processed user batch: {userBatch.Length} IDs.");
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

        foreach (var userBatch in userIds.Chunk(_batchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var transformedRecords =
                new List<TransformedUserEmployment>();

            var employmentRecords =
                _userEmploymentExtractor.ExtractAsync(
                    userBatch,
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

                transformedRecords.Add(transformed);

                if (transformed.IsRemoved)
                {
                    statistics.RecordsRemoved++;
                }
            }

            if (transformedRecords.Count > 0)
            {
                await _stagingRepository.InsertUserEmploymentsAsync(
                    transformedRecords,
                    cancellationToken);

                statistics.RecordsWritten +=
                    transformedRecords.Count;
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

        foreach (var userBatch in userIds.Chunk(_batchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var transformedRecords =
                new List<TransformedUserAdminLocation>();

            var records =
                _userAdminLocationExtractor.ExtractAsync(
                    userBatch,
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

                transformedRecords.Add(transformed);

                if (transformed.IsRemoved)
                {
                    statistics.RecordsRemoved++;
                }
            }

            if (transformedRecords.Count > 0)
            {
                await _stagingRepository.InsertUserAdminLocationsAsync(
                    transformedRecords,
                    cancellationToken);

                statistics.RecordsWritten +=
                    transformedRecords.Count;
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

        foreach (var userBatch in userIds.Chunk(_batchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var transformedRecords =
                new List<TransformedUserGroupReporter>();

            var records =
                _userGroupReporterExtractor.ExtractAsync(
                    userBatch,
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

                transformedRecords.Add(transformed);

                if (transformed.IsRemoved)
                {
                    statistics.RecordsRemoved++;
                }
            }

            if (transformedRecords.Count > 0)
            {
                await _stagingRepository.InsertUserGroupReportersAsync(
                    transformedRecords,
                    cancellationToken);

                statistics.RecordsWritten +=
                    transformedRecords.Count;
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

        var transformedRecords =
            new List<TransformedOrganisationType>();

        await foreach (
            var source in organisationTypes
                .WithCancellation(cancellationToken))
        {
            statistics.RecordsRead++;

            mappingDictionary.TryGetValue(
                source.Id,
                out var mapping);

            if (mapping is null || !mapping.IsMapped)
            {
                statistics.RecordsUnmapped++;

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    null,
                    "Warning",
                    "OrganisationTypeTransformer",
                    $"Legacy Organisation Type {source.Id} " +
                    $"('{source.Name}') does not have a valid Learning Hub mapping.");
            }

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

            transformedRecords.Add(transformed);

            if (transformed.IsRemoved)
            {
                statistics.RecordsRemoved++;
            }

            if (transformedRecords.Count >= _batchSize)
            {
                await _stagingRepository.InsertOrganisationTypesAsync(
                    transformedRecords,
                    cancellationToken);

                statistics.RecordsWritten +=
                    transformedRecords.Count;

                transformedRecords.Clear();
            }
        }

        if (transformedRecords.Count > 0)
        {
            await _stagingRepository.InsertOrganisationTypesAsync(
                transformedRecords,
                cancellationToken);

            statistics.RecordsWritten +=
                transformedRecords.Count;
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

        foreach (var locationBatch in locationIds.Chunk(_batchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var transformedRecords =
                new List<TransformedOrganisation>();

            var organisations =
                _organisationExtractor.ExtractAsync(
                    locationBatch,
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

                transformedRecords.Add(transformed);

                if (transformed.IsRemoved)
                {
                    statistics.RecordsRemoved++;
                }
            }

            if (transformedRecords.Count > 0)
            {
                await _stagingRepository.InsertOrganisationsAsync(
                    transformedRecords,
                    cancellationToken);

                statistics.RecordsWritten +=
                    transformedRecords.Count;
            }
        }

        return statistics;
    }
    private async Task<MigrationStatistics>
    TransformAndStageUserOrganisationsAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics = new MigrationStatistics();

        var userIds =
            await _learningHubRepository
                .GetUserIdsToMigrateAsync(
                    cancellationToken);

        var mappings =
            await _staffGroupJobRoleTypeMappingRepository
                .GetMappingsAsync(
                    cancellationToken);

        var mappingDictionary =
            mappings
                .Where(x =>
                    x.IsMapped &&
                    x.JobRoleTypeId.HasValue)
                .GroupBy(x => x.LegacyStaffGroupId)
                .ToDictionary(
                    x => x.Key,
                    x => x.First());

        foreach (var userBatch in userIds.Chunk(_batchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var transformedRecords =
                new List<TransformedUserOrganisation>();

            var employments =
                _userEmploymentExtractor.ExtractAsync(
                    userBatch,
                    cancellationToken);

            await foreach (
                var employment in employments
                    .WithCancellation(cancellationToken))
            {
                statistics.RecordsRead++;

                if (!employment.LocationId.HasValue)
                {
                    statistics.RecordsSkipped++;
                    continue;
                }

                if (!employment.JobRoleId.HasValue)
                {
                    statistics.RecordsSkipped++;
                    continue;
                }

                if (!mappingDictionary.TryGetValue(
                        employment.JobRoleId.Value,
                        out var mapping))
                {
                    statistics.RecordsSkipped++;
                    statistics.RecordsUnmapped++;
                    continue;
                }

                if (!mapping.JobRoleTypeId.HasValue)
                {
                    statistics.RecordsSkipped++;
                    statistics.RecordsUnmapped++;
                    continue;
                }

                var createDate =
                    employment.StartDate.HasValue
                        ? new DateTimeOffset(
                            employment.StartDate.Value,
                            TimeSpan.Zero)
                        : DateTimeOffset.UtcNow;

                DateTimeOffset? amendDate =
                        employment.AmendDate.HasValue
                            ? new DateTimeOffset(
                                employment.AmendDate.Value,
                                TimeSpan.Zero)
                            : null;

                var transformed =
                    _userOrganisationTransformer.Transform(
                        elfhUserId: employment.UserId,
                        elfhLocationId: employment.LocationId.Value,
                        userId: employment.UserId,
                        organisationId: employment.LocationId.Value,
                        jobRoleTypeId: mapping.JobRoleTypeId.Value,
                        jobRole: mapping.JobRoleType,
                        startDate: employment.StartDate.HasValue
                            ? new DateTimeOffset(
                                employment.StartDate.Value,
                                TimeSpan.Zero)
                            : null,
                        endDate: employment.EndDate.HasValue
                            ? new DateTimeOffset(
                                employment.EndDate.Value,
                                TimeSpan.Zero)
                            : null,
                        createDate: createDate,
                        createUserId: null,
                        amendDate: amendDate,
                        amendUserId: employment.AmendUserId,
                        removeDate: employment.Deleted
                            ? amendDate
                            : null,
                        removeUserId: employment.Deleted
                            ? employment.AmendUserId
                            : null,
                        migrationRunId: migrationRunId);

                transformedRecords.Add(transformed);

                if (employment.Deleted)
                {
                    statistics.RecordsRemoved++;
                }
            }

            if (transformedRecords.Count == 0)
            {
                continue;
            }

            await _stagingRepository.InsertUserOrganisationsAsync(
                transformedRecords,
                cancellationToken);

            statistics.RecordsWritten +=
                transformedRecords.Count;
        }

        return statistics;
    }
    private async Task<MigrationStatistics>
    TransformAndStageUserRolesAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics =
            new MigrationStatistics();

        var userIds =
            await _learningHubRepository
                .GetUserIdsToMigrateAsync(
                    cancellationToken);

        var roles =
            await _learningHubRepository
                .GetRolesAsync(
                    cancellationToken);

        var roleId =
            RoleResolver.ResolveUserAdminLocationRole(
                roles);

        foreach (var userBatch in userIds.Chunk(_batchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var transformedRecords =
                new List<TransformedUserRole>();

            var adminLocations =
                _userAdminLocationExtractor.ExtractAsync(
                    userBatch,
                    cancellationToken);

            await foreach (
                var adminLocation in adminLocations
                    .WithCancellation(cancellationToken))
            {
                statistics.RecordsRead++;

                if (!adminLocation.AdminLocationId.HasValue)
                {
                    statistics.RecordsSkipped++;
                    continue;
                }

                var transformed =
                    _userRoleTransformer.Transform(
                        adminLocation.UserId,
                        adminLocation.AdminLocationId,
                        roleId,
                        adminLocation.Deleted,
                        migrationRunId);

                transformedRecords.Add(
                    transformed);

                if (transformed.IsRemoved)
                {
                    statistics.RecordsRemoved++;
                }
            }

            if (transformedRecords.Count == 0)
            {
                continue;
            }

            await _stagingRepository.InsertUserRolesAsync(
                transformedRecords,
                cancellationToken);

            statistics.RecordsWritten +=
                transformedRecords.Count;
        }

        return statistics;
    }
    private async Task<MigrationStatistics>
    TransformAndStageUserGroupRolesAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics =
            new MigrationStatistics();

        var userIds =
            await _learningHubRepository
                .GetUserIdsToMigrateAsync(
                    cancellationToken);

        var roles =
            await _learningHubRepository
                .GetRolesAsync(
                    cancellationToken);

        var roleId =
            RoleResolver.ResolveUserGroupReporterRole(
                roles);

        foreach (var userBatch in userIds.Chunk(_batchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var transformedRecords =
                new List<TransformedUserGroupRole>();

            var reporters =
                _userGroupReporterExtractor.ExtractAsync(
                    userBatch,
                    cancellationToken);

            await foreach (
                var reporter in reporters
                    .WithCancellation(cancellationToken))
            {
                statistics.RecordsRead++;

                var transformed =
                    _userGroupRoleTransformer.Transform(
                        reporter.UserId,
                        reporter.UserGroupId,
                        reporter.UserGroupReporterId,
                        roleId,
                        reporter.Deleted,
                        migrationRunId);

                transformedRecords.Add(
                    transformed);

                if (transformed.IsRemoved)
                {
                    statistics.RecordsRemoved++;
                }
            }

            if (transformedRecords.Count == 0)
            {
                continue;
            }

            await _stagingRepository
                .InsertUserGroupRolesAsync(
                    transformedRecords,
                    cancellationToken);

            statistics.RecordsWritten +=
                transformedRecords.Count;
        }

        return statistics;
    }
}