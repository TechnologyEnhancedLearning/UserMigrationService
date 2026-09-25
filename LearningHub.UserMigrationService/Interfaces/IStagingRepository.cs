using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IStagingRepository
{
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
}