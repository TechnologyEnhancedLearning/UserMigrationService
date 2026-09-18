using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IUserEmploymentTransformer
{
    TransformedUserEmployment Transform(ElfhUserEmployment source,Guid migrationRunId);
}