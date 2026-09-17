using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IProfessionalBodyTransformer
{
    TransformedProfessionalBody Transform(ElfhProfessionalBody source,ProfessionalBodyMapping? mapping,Guid migrationRunId);
}