
namespace LearningHub.UserMigrationService.Interfaces;

public interface IUserExternalSystemReferenceResolver
{
    Task<int?> ResolveUserIdAsync(
        int legacyUserId,
        CancellationToken cancellationToken);

    Task<int?> ResolveAmendUserIdAsync(
        int legacyAmendUserId,
        CancellationToken cancellationToken);

    Task<int?> ResolveExternalSystemIdAsync(
        int legacyExternalSystemId,
        CancellationToken cancellationToken);
}