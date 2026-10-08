using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IGdcRegisterTransformer
{
    TransformedGdcRegister Transform(
        ElfhGdcRegister source,
        Guid migrationRunId);
}