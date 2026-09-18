using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IUserTransformer
{
    TransformedUser Transform(ElfhUser source,  Guid migrationRunId);
}