using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IStagingRepository
{
    Task InsertProfessionalBodyAsync(TransformedProfessionalBody professionalBody,CancellationToken cancellationToken = default);
}