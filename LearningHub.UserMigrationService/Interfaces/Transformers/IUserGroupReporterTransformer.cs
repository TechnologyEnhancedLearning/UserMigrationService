using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IUserGroupReporterTransformer
{
    TransformedUserGroupReporter Transform(ElfhUserGroupReporter source,Guid migrationRunId);
}