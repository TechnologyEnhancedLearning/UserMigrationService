using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Interfaces.Extractors;
using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models;
using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;
using LearningHub.UserMigrationService.Services;
using LearningHub.UserMigrationService.Transformers;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace LearningHub.UserMigrationService.Pipeline;

public class MigrationPipeline : IMigrationPipeline
{

    private readonly IMigrationLogger _migrationLogger;
    private readonly IUserMigrationSelectionService _userMigrationSelectionService;
    private readonly IOrganisationMigrationSelectionService _organisationMigrationSelectionService;
    private readonly ISupportingUserTablesMigrationService _supportingUserTablesMigrationService;
    private readonly IMigrationMappingInitializer _migrationMappingInitializer;

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
    private readonly IGdcRegisterTransformer _gdcRegisterTransformer;
    private readonly IGmcRegisterTransformer _gmcRegisterTransformer;

    private readonly ILearningHubRepository _learningHubRepository;
    private readonly ILegacyRepository _legacyRepository;
    private readonly IStagingRepository _stagingRepository;
    private readonly IOrganisationTypeMappingRepository _organisationTypeMappingRepository;
    private readonly IStaffGroupJobRoleTypeMappingRepository _staffGroupJobRoleTypeMappingRepository;
    private readonly IValidationIssueRepository _validationIssueRepository;

    private readonly IFinalLoadService _finalLoadService;

    private readonly bool _enableValidation;
    private readonly int _batchSize;

    public MigrationPipeline(
        IMigrationMappingInitializer migrationMappingInitializer,

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
        IGdcRegisterTransformer gdcRegisterTransformer,
        IGmcRegisterTransformer gmcRegisterTransformer,

        IMigrationLogger migrationLogger,
        IUserMigrationSelectionService userMigrationSelectionService,
        IOrganisationMigrationSelectionService organisationMigrationSelectionService,
        ISupportingUserTablesMigrationService supportingUserTablesMigrationService,

        ILegacyRepository legacyRepository,
        ILearningHubRepository learningHubRepository,
        IOrganisationTypeMappingRepository organisationTypeMappingRepository,
        IStagingRepository stagingRepository,
        IStaffGroupJobRoleTypeMappingRepository staffGroupJobRoleTypeMappingRepository,
        IValidationIssueRepository validationIssueRepository,
        IFinalLoadService finalLoadService,
        IOptions<MigrationOptions> migrationOptions)
    {

        _migrationMappingInitializer = migrationMappingInitializer;

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
        _gdcRegisterTransformer = gdcRegisterTransformer;
        _gmcRegisterTransformer = gmcRegisterTransformer;

        _migrationLogger = migrationLogger;
        _userMigrationSelectionService = userMigrationSelectionService;
        _organisationMigrationSelectionService = organisationMigrationSelectionService;
        _supportingUserTablesMigrationService = supportingUserTablesMigrationService;
        _organisationTypeMappingRepository = organisationTypeMappingRepository;
        _learningHubRepository = learningHubRepository;
        _legacyRepository = legacyRepository;
        _stagingRepository = stagingRepository;
        _staffGroupJobRoleTypeMappingRepository = staffGroupJobRoleTypeMappingRepository;
        _validationIssueRepository = validationIssueRepository;

        _finalLoadService = finalLoadService;

        _enableValidation =migrationOptions.Value.EnableValidation;
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
            // Step 4.5 - Initialise migration mappings
            // ==================================================

            var mappingStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Initialise Migration Mappings");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    mappingStepId,
                    "Information",
                    "MigrationMappingInitializer",
                    "Initialising migration mapping tables.");

                await _migrationMappingInitializer.InitialiseAsync(
                    cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    mappingStepId,
                    new MigrationStatistics());

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    mappingStepId,
                    "Information",
                    "MigrationMappingInitializer",
                    "Migration mapping tables initialised.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    mappingStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    mappingStepId,
                    "Error",
                    "MigrationMappingInitializer",
                    "Migration mapping initialisation failed.",
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

            // ==================================================
            // Step 6.5 - Extract and stage supporting user tables
            // ==================================================

            var supportingUserTablesStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Extract and Stage Supporting User Tables");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    supportingUserTablesStepId,
                    "Information",
                    "SupportingUserTablesMigrationService",
                    "Extracting and staging UserExternalSystem, UserPasswordValidationTokens, and MergeUserLearningHistoryLog for selected users.");

                var supportingUserTablesStatistics =
                    await _supportingUserTablesMigrationService.ExtractAndStageAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    supportingUserTablesStepId,
                    supportingUserTablesStatistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    supportingUserTablesStepId,
                    "Information",
                    "SupportingUserTablesMigrationService",
                    $"Supporting user tables staged. Read: {supportingUserTablesStatistics.RecordsRead}; written: {supportingUserTablesStatistics.RecordsWritten}; skipped: {supportingUserTablesStatistics.RecordsSkipped}.");

                Console.WriteLine(
                    "Supporting user tables extraction and staging completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    supportingUserTablesStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    supportingUserTablesStepId,
                    "Error",
                    "SupportingUserTablesMigrationService",
                    "Supporting user tables extraction and staging failed.",
                    ex);

                throw;
            }

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
            // Step 11 - Transform and stage GDC Registers
            // ==================================================

            var gdcRegisterStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Transform and Stage GDC Registers");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    gdcRegisterStepId,
                    "Information",
                    "GdcRegisterTransformer",
                    "Starting GDC Register extraction, transformation and staging.");

                var gdcStatistics =
                    await TransformAndStageGdcRegistersAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    gdcRegisterStepId,
                    gdcStatistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    gdcRegisterStepId,
                    "Information",
                    "GdcRegisterTransformer",
                    "GDC Register extraction, transformation and staging completed.");

                Console.WriteLine(
                    "GDC Register transformation and staging completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    gdcRegisterStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    gdcRegisterStepId,
                    "Error",
                    "GdcRegisterTransformer",
                    "GDC Register transformation and staging failed.",
                    ex);

                throw;
            }


            // ==================================================
            // Step 12 - Transform and stage GMC 
            // ==================================================

            var gmcRegisterStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Transform and Stage GMC Registers");

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    gmcRegisterStepId,
                    "Information",
                    "GmcRegisterTransformer",
                    "Starting GMC Register extraction, transformation and staging.");

                var gmcStatistics =
                    await TransformAndStageGmcRegistersAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    gmcRegisterStepId,
                    gmcStatistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    gmcRegisterStepId,
                    "Information",
                    "GmcRegisterTransformer",
                    "GMC Register extraction, transformation and staging completed.");

                Console.WriteLine(
                    "GMC Register transformation and staging completed.");
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    gmcRegisterStepId,
                    ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    gmcRegisterStepId,
                    "Error",
                    "GmcRegisterTransformer",
                    "GMC Register transformation and staging failed.",
                    ex);

                throw;
            }

            
            // ==================================================
            // Step 13 - Transform and stage Organisations
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
            // Step 14 - Transform and stage User Organisations
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
            // Step 15 - Transform and stage User Roles
            // ==================================================
            var userRoleStepId =
                await _migrationLogger.StartStepAsync(
                    migrationRunId,
                    "Transform and Stage User Roles");

            try
            {
                var statistics =
                    await TransformAndStageUserRolesAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    userRoleStepId,
                    statistics);
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    userRoleStepId,
                    ex);

                throw;
            }

            // ==================================================
            // Step 16 - Transform and stage User Group Roles
            // ==================================================

            var userGroupRoleStepId =
                    await _migrationLogger.StartStepAsync(
                        migrationRunId,
                        "Transform and Stage User Group Roles");

            try
            {
                var statistics =
                    await TransformAndStageUserGroupRolesAsync(
                        migrationRunId,
                        cancellationToken);

                await _migrationLogger.CompleteStepAsync(
                    userGroupRoleStepId,
                    statistics);
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(
                    userGroupRoleStepId,
                    ex);

                throw;
            }
            
            // ==================================================
            // Step 18 - Load staged data into Learning Hub tables
            // ==================================================

                await ExecuteFinalLoadAsync(
                    migrationRunId,
                    cancellationToken);

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
    


    private async Task<MigrationStatistics> TransformAndStageUsersAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics = new MigrationStatistics();

        var userIds = await _learningHubRepository
            .GetUserIdsToMigrateAsync(cancellationToken);

        // Build legacy ID -> actual Learning Hub Professional Body ID.
        var professionalBodyIdMap =
            await BuildProfessionalBodyIdMapAsync(
                cancellationToken);

        foreach (var userBatch in userIds.Chunk(_batchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Key: legacy user ID.
            // Value: legacy jobRoleTBL.medicalCouncilId.
            var legacyProfessionalBodyIds =
                await _legacyRepository.GetProfessionalBodyIdsAsync(
                    userBatch,
                    cancellationToken);

            var transformedUsers = new List<TransformedUser>(_batchSize);

            var users = _userExtractor.ExtractAsync(
                userBatch,
                cancellationToken);

            await foreach (var source in users.WithCancellation(cancellationToken))
            {
                statistics.RecordsRead++;

                var transformed = _userTransformer.Transform(
                    source,
                    migrationRunId);

                // Resolve the legacy Professional Body ID directly
                // against the seeded Learning Hub Professional Body IDs.
                transformed.ProfessionalBodyId = null;

                if (legacyProfessionalBodyIds.TryGetValue(
                        source.UserId,
                        out var legacyProfessionalBodyId)
                    && legacyProfessionalBodyId.HasValue)
                {
                    var legacyId = legacyProfessionalBodyId.Value;

                    if (professionalBodyIdMap.TryGetValue(
                            legacyId,
                            out var destinationId))
                    {
                        transformed.ProfessionalBodyId = destinationId;
                    }
                    else
                    {
                        statistics.RecordsUnmapped++;

                        await _migrationLogger.LogAsync(
                            migrationRunId,
                            null,
                            "Warning",
                            "UserTransformer",
                            $"No Learning Hub Professional Body mapping exists " +
                            $"for legacy Professional Body ID {legacyId}, " +
                            $"used by legacy user {source.UserId}.");
                    }
                }

                var validationErrors = _enableValidation
                    ? TransformationValidator.Validate(transformed)
                    : Array.Empty<string>();

                if (validationErrors.Count > 0)
                {
                    statistics.RecordsFailed++;

                    await RecordValidationIssuesAsync(
                        migrationRunId,
                        "migrations.Users",
                        source.UserId,
                        validationErrors,
                        cancellationToken);

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

                statistics.RecordsWritten += transformedUsers.Count;
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
        var statistics = new MigrationStatistics();

        var mappings = await _organisationTypeMappingRepository
            .GetMappingsAsync(cancellationToken);

        var mappingDictionary = mappings.ToDictionary(
            x => x.LegacyOrganisationTypeId);

        var organisationTypes =
            _supportingLookupExtractor.ExtractOrganisationTypesAsync(
                cancellationToken);

        var transformedRecords =
            new List<TransformedOrganisationType>();

        // Log each missing or low-confidence legacy type only once.
        var loggedLegacyTypeIds = new HashSet<int>();

        await foreach (var source in organisationTypes
            .WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            statistics.RecordsRead++;

            mappingDictionary.TryGetValue(source.Id, out var mapping);

            var isMapped = mapping is not null;

            if (!isMapped && loggedLegacyTypeIds.Add(source.Id))
            {
                statistics.RecordsUnmapped++;

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    null,
                    "Warning",
                    "OrganisationTypeTransformer",
                    $"Legacy Organisation Type {source.Id} " +
                    $"('{source.Name}') does not have a mapping.");
            }

            var transformed = _organisationTypeTransformer.Transform(
                source,
                mapping?.OrganisationTypeId,
                mapping?.OrganisationType,
                isMapped,
                migrationRunId);

            var validationErrors = _enableValidation
                ? TransformationValidator.Validate(transformed)
                : Array.Empty<string>();

            if (validationErrors.Count > 0)
            {
                statistics.RecordsFailed++;

                await RecordValidationIssuesAsync(
                    migrationRunId,
                    "migrations.OrganisationType",
                    source.Id,
                    validationErrors,
                    cancellationToken);

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

                statistics.RecordsWritten += transformedRecords.Count;

                transformedRecords.Clear();
            }
        }

        if (transformedRecords.Count > 0)
        {
            await _stagingRepository.InsertOrganisationTypesAsync(
                transformedRecords,
                cancellationToken);

            statistics.RecordsWritten += transformedRecords.Count;
        }

        return statistics;
    }
    private async Task<MigrationStatistics> TransformAndStageOrganisationsAsync(
    Guid migrationRunId,
    CancellationToken cancellationToken)
    {
        var statistics = new MigrationStatistics();

        const int fallbackOrganisationTypeId = 23;

        // Get valid Organisation Type IDs from Learning Hub.
        var validOrganisationTypeIds =
            (await _learningHubRepository.GetOrganisationTypeIdsAsync(
                cancellationToken)).ToHashSet();

        // Check that the agreed fallback exists.
        if (!validOrganisationTypeIds.Contains(fallbackOrganisationTypeId))
        {
            throw new InvalidOperationException(
                $"Fallback Organisation Type ID {fallbackOrganisationTypeId} " +
                "does not exist in hub.OrganisationType.");
        }

        // Load the mapping table once.
        var mappings = await _organisationTypeMappingRepository
            .GetMappingsAsync(cancellationToken);

        // Ignore legacy ID 0 because it represents an unknown type.
        var mappingDictionary = mappings
            .Where(x => x.LegacyOrganisationTypeId > 0)
            .ToDictionary(x => x.LegacyOrganisationTypeId);

        // Get the organisation locations selected for migration.
        var locationIds = await _learningHubRepository
            .GetOrganisationLocationIdsToMigrateAsync(cancellationToken);

        // Avoid generating repeated log entries for the same legacy type.
        var loggedManualReviewTypes = new HashSet<int>();

        foreach (var locationBatch in locationIds
            .Distinct()
            .Chunk(_batchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Infer regions for this batch.
            var inferredRegions =
                await _legacyRepository.GetInferredOrganisationRegionsAsync(
                    locationBatch,
                    cancellationToken);

            var transformedRecords =
                new List<TransformedOrganisation>();

            var organisations = _organisationExtractor.ExtractAsync(
                locationBatch,
                cancellationToken);

            await foreach (var source in organisations
                .WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                statistics.RecordsRead++;

                // Default to the agreed fallback Organisation Type ID.
                var organisationTypeId = fallbackOrganisationTypeId;
                var requiresManualReview = true;

                var legacyOrganisationTypeId =
                    source.LocationTypeId.GetValueOrDefault();

                // Resolve the mapping when a valid legacy ID is available.
                if (legacyOrganisationTypeId > 0 &&
                    mappingDictionary.TryGetValue(
                        legacyOrganisationTypeId,
                        out var mapping))
                {
                    // Only accept high/medium-confidence mappings.
                    if (mapping.MappingConfidence is 1 or 2 &&
                        validOrganisationTypeIds.Contains(
                            mapping.OrganisationTypeId))
                    {
                        organisationTypeId = mapping.OrganisationTypeId;
                        requiresManualReview = false;
                    }
                }

                // Record unmapped, low-confidence and invalid mappings.
                if (requiresManualReview)
                {
                    statistics.RecordsUnmapped++;

                    // Log once per legacy type rather than once per organisation.
                    if (loggedManualReviewTypes.Add(
                        legacyOrganisationTypeId))
                    {
                        var message =
                            $"Organisation type requires manual review. " +
                            $"Legacy LocationTypeId: " +
                            $"{source.LocationTypeId?.ToString() ?? "NULL"}. " +
                            $"Assigned fallback OrganisationType ID " +
                            $"{fallbackOrganisationTypeId}.";

                        await _migrationLogger.LogAsync(
                            migrationRunId,
                            null,
                            "Warning",
                            "OrganisationTransformer",
                            message);

                        await RecordValidationIssuesAsync(
                            migrationRunId,
                            "migrations.Organisations",
                            source.LocationId,
                            new[] { message },
                            cancellationToken);
                    }
                }

                // Transform the organisation using the resolved type ID.
                var transformed = _organisationTransformer.Transform(
                    source,
                    organisationTypeId,
                    migrationRunId);

                // Apply the inferred region.
                transformed.RegionId =
                    inferredRegions.TryGetValue(
                        source.LocationId,
                        out var inferredRegionId)
                        ? inferredRegionId
                        : null;

                // Validate the transformed organisation.
                var validationErrors = _enableValidation
                    ? TransformationValidator.Validate(transformed)
                    : Array.Empty<string>();

                if (validationErrors.Count > 0)
                {
                    statistics.RecordsFailed++;

                    await RecordValidationIssuesAsync(
                        migrationRunId,
                        "migrations.Organisations",
                        source.LocationId,
                        validationErrors,
                        cancellationToken);

                    foreach (var error in validationErrors)
                    {
                        await _migrationLogger.LogAsync(
                            migrationRunId,
                            null,
                            "Warning",
                            "OrganisationTransformer",
                            $"Legacy Organisation {source.LocationId}: {error}");
                    }

                    continue;
                }

                transformedRecords.Add(transformed);

                if (transformed.RemoveUserId.HasValue)
                {
                    statistics.RecordsRemoved++;
                }
            }

            // Insert the transformed organisations into staging.
            if (transformedRecords.Count > 0)
            {
                await _stagingRepository.InsertOrganisationsAsync(
                    transformedRecords,
                    cancellationToken);

                statistics.RecordsWritten += transformedRecords.Count;
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
        var jobRoleStaffGroupMap =
             await _supportingLookupExtractor
                     .ExtractJobRoleStaffGroupMapAsync(cancellationToken);
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

                if (!jobRoleStaffGroupMap.TryGetValue(
                        employment.JobRoleId.Value,
                        out var staffGroupId) ||
                                 !mappingDictionary.TryGetValue(
                        staffGroupId,
                        out var mapping))
                {
                    statistics.RecordsSkipped++;
                    statistics.RecordsUnmapped++;

                    await RecordValidationIssuesAsync(
                        migrationRunId,
                        "migrations.UserOrganisations",
                        employment.UserEmploymentId,
                        new[]
                        {
                                        $"No StaffGroupJobRoleType mapping exists " +
                                        $"for legacy JobRoleId " +
                                        $"{employment.JobRoleId}."
                                        },
                                        cancellationToken);

                    continue;
                }

                if (!mapping.JobRoleTypeId.HasValue)
                {
                    statistics.RecordsSkipped++;
                    statistics.RecordsUnmapped++;
                    continue;
                }
                var createDate =
                    employment.StartDate ?? DateTimeOffset.UtcNow;

                DateTimeOffset? amendDate =
                    employment.AmendDate;

                var transformed =
                    _userOrganisationTransformer.Transform(
                        elfhUserId: employment.UserId,
                        elfhLocationId: employment.LocationId.Value,
                        userId: employment.UserId,
                        organisationId: employment.LocationId.Value,
                        jobRoleTypeId: mapping.JobRoleTypeId.Value,
                        jobRole: mapping.JobRoleType,
                        startDate: employment.StartDate ?? DateTimeOffset.UtcNow,
                        endDate: employment.EndDate,
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
                var validationErrors =
                        _enableValidation
                            ? TransformationValidator.Validate(
                                transformed)
                            : Array.Empty<string>();

                if (validationErrors.Count > 0)
                {
                    statistics.RecordsFailed++;

                    await RecordValidationIssuesAsync(
                        migrationRunId,
                        "migrations.UserOrganisations",
                        employment.UserEmploymentId,
                        validationErrors,
                        cancellationToken);

                    foreach (var error in validationErrors)
                    {
                        await _migrationLogger.LogAsync(
                            migrationRunId,
                            null,
                            "Warning",
                            "UserOrganisationTransformer",
                            $"Legacy User Employment " +
                            $"{employment.UserEmploymentId}: {error}");
                    }

                    continue;
                }
                transformedRecords.Add(transformed);

                if (employment.Deleted)
                {
                    statistics.RecordsRemoved++;
                }
            }
            var beforeDedupe = transformedRecords.Count;

            transformedRecords = transformedRecords
                .GroupBy(x => new { x.UserId, x.OrganisationId, x.JobRoleTypeId })
                .Select(g => g
                    .OrderBy(x => x.RemoveDate.HasValue)
                    .ThenByDescending(x => x.AmendDate ?? x.CreateDate)
                    .First())
                .ToList();

            statistics.RecordsSkipped += beforeDedupe - transformedRecords.Count;
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
    private async Task RecordValidationIssuesAsync(
    Guid migrationRunId,
    string tableName,
    int? elfhRecordId,
    IReadOnlyList<string> errors,
    CancellationToken cancellationToken)
    {
        if (!_enableValidation ||
            errors.Count == 0)
        {
            return;
        }

        var issues =
            errors.Select(
                error => new ValidationIssue
                {
                    MigrationRunId = migrationRunId,

                    TableName = tableName,

                    StagingRecordId = null,

                    ElfhRecordId = elfhRecordId,

                    ValidationType = "Transformation",

                    Severity = "Error",

                    ColumnName = null,

                    Message = error,

                    CreatedUtc = DateTime.UtcNow
                })
            .ToList();

        await _validationIssueRepository.InsertAsync(
            issues,
            cancellationToken);
    }
    private async Task<MigrationStatistics>
     TransformAndStageGdcRegistersAsync(
         Guid migrationRunId,
         CancellationToken cancellationToken)
    {
        var statistics =
            new MigrationStatistics();

        var transformedRecords =
            new List<TransformedGdcRegister>();

        await foreach (
            var source in
            _supportingLookupExtractor
                .ExtractGdcAsync(cancellationToken)
                .WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            statistics.RecordsRead++;

            if (string.IsNullOrWhiteSpace(
                    source.RegistrationNumber))
            {
                statistics.RecordsSkipped++;

                continue;
            }

            TransformedGdcRegister transformed;

            try
            {
                transformed =
                    _gdcRegisterTransformer.Transform(
                        source,
                        migrationRunId);
            }
            catch (Exception ex)
            {
                statistics.RecordsFailed++;

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    null,
                    "Error",
                    "GdcRegisterTransformer",
                    $"Failed to transform GDC register " +
                    $"'{source.RegistrationNumber}'.",
                    ex);

                continue;
            }

            transformedRecords.Add(
                transformed);

            if (transformedRecords.Count >= _batchSize)
            {
                await _stagingRepository
                    .InsertGdcRegistersAsync(
                        transformedRecords,
                        cancellationToken);

                statistics.RecordsWritten +=
                    transformedRecords.Count;

                transformedRecords.Clear();
            }
        }

        if (transformedRecords.Count > 0)
        {
            await _stagingRepository
                .InsertGdcRegistersAsync(
                    transformedRecords,
                    cancellationToken);

            statistics.RecordsWritten +=
                transformedRecords.Count;
        }

        return statistics;
    }
    private async Task<MigrationStatistics>
    TransformAndStageGmcRegistersAsync(
        Guid migrationRunId,
        CancellationToken cancellationToken)
    {
        var statistics =
            new MigrationStatistics();

        var transformedRecords =
            new List<TransformedGmcRegister>();

        await foreach (
            var source in
            _supportingLookupExtractor
                .ExtractGmcAsync(cancellationToken)
                .WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            statistics.RecordsRead++;

            if (string.IsNullOrWhiteSpace(
                    source.GmcReferenceNumber))
            {
                statistics.RecordsSkipped++;

                continue;
            }

            TransformedGmcRegister transformed;

            try
            {
                transformed =
                    _gmcRegisterTransformer.Transform(
                        source,
                        migrationRunId);
            }
            catch (Exception ex)
            {
                statistics.RecordsFailed++;

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    null,
                    "Error",
                    "GmcRegisterTransformer",
                    $"Failed to transform GMC register " +
                    $"'{source.GmcReferenceNumber}'.",
                    ex);

                continue;
            }

            transformedRecords.Add(
                transformed);

            if (transformedRecords.Count >= _batchSize)
            {
                await _stagingRepository
                    .InsertGmcRegistersAsync(
                        transformedRecords,
                        cancellationToken);

                statistics.RecordsWritten +=
                    transformedRecords.Count;

                transformedRecords.Clear();
            }
        }

        if (transformedRecords.Count > 0)
        {
            await _stagingRepository
                .InsertGmcRegistersAsync(
                    transformedRecords,
                    cancellationToken);

            statistics.RecordsWritten +=
                transformedRecords.Count;
        }

        return statistics;
    }
    private async Task ExecuteFinalLoadAsync(
    Guid migrationRunId,
    CancellationToken cancellationToken)
    {
        var procedures = new[]
        {
        "migrations.MergeUsers",
        "migrations.MergeOrganisations",
        "migrations.MergeUserOrganisations",
        "migrations.MergeUserExternalSystems",
        "migrations.MergeUserPasswordValidationTokens",
        "migrations.MergeUserLearningHistoryLogs",
        "migrations.MergeUserRoles"
    };

        foreach (var procedureName in procedures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var stepName = $"Final Load: {procedureName}";

            var stepId = await _migrationLogger.StartStepAsync(
                migrationRunId,
                stepName);

            try
            {
                await _migrationLogger.LogAsync(
                    migrationRunId,
                    stepId,
                    "Information",
                    "FinalLoadService",
                    $"Executing stored procedure {procedureName}.");

                var result = await _finalLoadService.ExecuteAsync(
                    procedureName,
                    migrationRunId,
                    cancellationToken);

                var statistics = result.ToMigrationStatistics();

                await _migrationLogger.CompleteStepAsync(
                    stepId,
                    statistics);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    stepId,
                    "Information",
                    "FinalLoadService",
                    $"{procedureName} completed. " +
                    $"Read: {result.RecordsRead}, " +
                    $"inserted: {result.RecordsInserted}, " +
                    $"updated: {result.RecordsUpdated}, " +
                    $"skipped: {result.RecordsSkipped}, " +
                    $"failed: {result.RecordsFailed}.");

                if (result.RecordsFailed > 0)
                {
                    throw new InvalidOperationException(
                        $"Stored procedure {procedureName} reported " +
                        $"{result.RecordsFailed} failed records.");
                }
            }
            catch (Exception ex)
            {
                await _migrationLogger.FailStepAsync(stepId, ex);

                await _migrationLogger.LogAsync(
                    migrationRunId,
                    stepId,
                    "Error",
                    "FinalLoadService",
                    $"Final load failed for {procedureName}.",
                    ex);

                throw;
            }
        }
    }
    private async Task<Dictionary<int, int>>
    BuildProfessionalBodyIdMapAsync(
        CancellationToken cancellationToken)
    {
        var destinationBodies =
            await _learningHubRepository
                .GetProfessionalBodiesAsync(cancellationToken);

        static string Normalize(string? value) =>
            new string(
                (value ?? string.Empty)
                    .Where(char.IsLetterOrDigit)
                    .Select(char.ToUpperInvariant)
                    .ToArray());

        // Destination lookup: normalized name -> actual LH ID.
        var destinationByName = destinationBodies
            .GroupBy(x => Normalize(x.ProfessionalBodyName))
            .ToDictionary(
                group => group.Key,
                group => group.First().Id);

        // Explicit aliases for known legacy names/codes.
        // Add further aliases only after checking the source data.
        var aliases = new Dictionary<string, string>
        {
            [Normalize("GMC")] =
                Normalize("General Medical Council (GMC)"),

            [Normalize("General Medical Council")] =
                Normalize("General Medical Council (GMC)"),

            [Normalize("NMC")] =
                Normalize("Nursing and Midwifery Council (NMC)"),

            [Normalize("Nursing and Midwifery Council")] =
                Normalize("Nursing and Midwifery Council (NMC)"),

            [Normalize("HCPC")] =
                Normalize("Health and Care Professions Council (HCPC)"),

            [Normalize("Health and Care Professions Council")] =
                Normalize("Health and Care Professions Council (HCPC)"),

            [Normalize("GDC")] =
                Normalize("General Dental Council (GDC)"),

            [Normalize("General Dental Council")] =
                Normalize("General Dental Council (GDC)"),

            [Normalize("GPhC")] =
                Normalize("General Pharmaceutical Council (GPhC)"),

            [Normalize("General Pharmaceutical Council")] =
                Normalize("General Pharmaceutical Council (GPhC)"),

            [Normalize("GCC")] =
                Normalize("General Chiropractic Council (GCC)"),

            [Normalize("General Chiropractic Council")] =
                Normalize("General Chiropractic Council (GCC)"),

            [Normalize("GOsC")] =
                Normalize("General Osteopathic Council (GOsC)"),

            [Normalize("General Osteopathic Council")] =
                Normalize("General Osteopathic Council (GOsC)"),

            [Normalize("GOC")] =
                Normalize("General Optical Council (GOC)"),

            [Normalize("General Optical Council")] =
                Normalize("General Optical Council (GOC)"),

            [Normalize("Other")] =
                Normalize("Other (not listed)")
        };

        var legacyToDestination = new Dictionary<int, int>();

        await foreach (
            var source in _professionalBodyExtractor
                .ExtractAsync(cancellationToken)
                .WithCancellation(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (source.ProfessionalBodyId <= 0)
            {
                continue;
            }

            var sourceKeys = new[]
            {
            Normalize(source.ProfessionalBodyCode),
            Normalize(source.ProfessionalBody)
        };

            int? destinationId = null;

            foreach (var key in sourceKeys.Where(
                         key => !string.IsNullOrWhiteSpace(key)))
            {
                var destinationKey = aliases.TryGetValue(
                    key,
                    out var canonicalName)
                    ? canonicalName
                    : key;

                if (destinationByName.TryGetValue(
                        destinationKey,
                        out var matchedId))
                {
                    destinationId = matchedId;
                    break;
                }
            }

            if (destinationId.HasValue)
            {
                legacyToDestination[source.ProfessionalBodyId] =
                    destinationId.Value;
            }
        }

        return legacyToDestination;
    }
}