using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IProfessionalBodyMappingRepository
{
    Task<IReadOnlyList<ProfessionalBodyMapping>> GetMappingsAsync(CancellationToken cancellationToken = default);
}