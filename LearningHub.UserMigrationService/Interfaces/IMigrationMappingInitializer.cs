namespace LearningHub.UserMigrationService.Interfaces;

public interface IMigrationMappingInitializer
{
    Task InitialiseAsync(CancellationToken cancellationToken = default);
}