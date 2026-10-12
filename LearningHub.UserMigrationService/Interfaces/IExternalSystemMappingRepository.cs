
namespace LearningHub.UserMigrationService.Interfaces;

public interface IExternalSystemMappingRepository
{
    Task<int?> ResolveExternalSystemIdAsync(
        int legacyExternalSystemId,
        CancellationToken cancellationToken);
}
