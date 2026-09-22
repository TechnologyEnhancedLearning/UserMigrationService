using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IStagingRepository
{
    Task InsertProfessionalBodyAsync(
        TransformedProfessionalBody professionalBody,
        CancellationToken cancellationToken = default);

    Task InsertUserAsync(
        TransformedUser user,
        CancellationToken cancellationToken = default);

    Task InsertUserEmploymentAsync(
        TransformedUserEmployment employment,
        CancellationToken cancellationToken = default);

    Task InsertUserAdminLocationAsync(
        TransformedUserAdminLocation adminLocation,
        CancellationToken cancellationToken = default);

    Task InsertUserGroupReporterAsync(
        TransformedUserGroupReporter groupReporter,
        CancellationToken cancellationToken = default);

    Task InsertOrganisationAsync(
        TransformedOrganisation organisation,
        CancellationToken cancellationToken = default);

    Task InsertOrganisationTypeAsync(
        TransformedOrganisationType organisationType,
        CancellationToken cancellationToken = default);
}