using LearningHub.UserMigrationService.Models.Transformation;
using LearningHub.UserMigrationService.Models.Staging;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IStagingRepository
{
    Task InsertUserExternalSystemsAsync(
        Guid migrationRunId,
        IReadOnlyCollection<UserExternalSystemStagingRecord> records,
        CancellationToken cancellationToken = default);

    Task InsertUserPasswordValidationTokensAsync(
        Guid migrationRunId,
        IReadOnlyCollection<UserPasswordValidationTokenStagingRecord> records,
        CancellationToken cancellationToken = default);

    Task InsertMergeUserLearningHistoryLogsAsync(
        Guid migrationRunId,
        IReadOnlyCollection<MergeUserLearningHistoryLogStagingRecord> records,
        CancellationToken cancellationToken = default);

    Task InsertProfessionalBodiesAsync(
        IReadOnlyCollection<TransformedProfessionalBody> professionalBodies,
        CancellationToken cancellationToken = default);

    Task InsertUsersAsync(
        IReadOnlyCollection<TransformedUser> users,
        CancellationToken cancellationToken = default);

    Task InsertUserEmploymentsAsync(
        IReadOnlyCollection<TransformedUserEmployment> employments,
        CancellationToken cancellationToken = default);

    Task InsertUserAdminLocationsAsync(
        IReadOnlyCollection<TransformedUserAdminLocation> adminLocations,
        CancellationToken cancellationToken = default);

    Task InsertUserGroupReportersAsync(
        IReadOnlyCollection<TransformedUserGroupReporter> groupReporters,
        CancellationToken cancellationToken = default);

    Task InsertOrganisationsAsync(
        IReadOnlyCollection<TransformedOrganisation> organisations,
        CancellationToken cancellationToken = default);

    Task InsertOrganisationTypesAsync(
        IReadOnlyCollection<TransformedOrganisationType> organisationTypes,
        CancellationToken cancellationToken = default);
    Task InsertUserOrganisationsAsync(
    IReadOnlyCollection<TransformedUserOrganisation> userOrganisations,
    CancellationToken cancellationToken = default);

    Task InsertUserRolesAsync(
        IReadOnlyCollection<TransformedUserRole> userRoles,
        CancellationToken cancellationToken = default);

    Task InsertUserGroupRolesAsync(
        IReadOnlyCollection<TransformedUserGroupRole> userGroupRoles,
        CancellationToken cancellationToken = default);
    Task InsertGdcRegistersAsync(
        IEnumerable<TransformedGdcRegister> records,
        CancellationToken cancellationToken);

    Task InsertGmcRegistersAsync(
        IEnumerable<TransformedGmcRegister> records,
        CancellationToken cancellationToken);
}