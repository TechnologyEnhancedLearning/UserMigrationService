using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IUserAdminLocationTransformer
{
    TransformedUserAdminLocation Transform( ElfhUserAdminLocation source,Guid migrationRunId);
}