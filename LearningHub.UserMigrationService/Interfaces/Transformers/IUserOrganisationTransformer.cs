using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces.Transformers;

public interface IUserOrganisationTransformer
{
    TransformedUserOrganisation Transform(
        int elfhUserId,
        int elfhLocationId,
        int userId,
        int organisationId,
        int jobRoleTypeId,
        string? jobRole,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        DateTimeOffset createDate,
        int? createUserId,
        DateTimeOffset? amendDate,
        int? amendUserId,
        DateTimeOffset? removeDate,
        int? removeUserId,
        Guid migrationRunId);
}