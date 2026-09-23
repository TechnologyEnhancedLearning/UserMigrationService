using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IOrganisationTypeMappingRepository
{
    Task<IReadOnlyList<OrganisationTypeMapping>> GetMappingsAsync(CancellationToken cancellationToken = default);
}