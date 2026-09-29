using LearningHub.UserMigrationService.Interfaces.Transformers;
using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Transformers;

public class UserOrganisationTransformer
    : IUserOrganisationTransformer
{
    public TransformedUserOrganisation Transform(
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
        Guid migrationRunId)
    {
        return new TransformedUserOrganisation
        {
            MigrationRunId = migrationRunId,

            ElfhUserId = elfhUserId,
            ElfhLocationId = elfhLocationId,

            UserId = userId,
            OrganisationId = organisationId,

            JobRoleTypeId = jobRoleTypeId,
            JobRole = jobRole,

            StartDate = startDate,
            EndDate = endDate,

            CreateDate = createDate,
            CreateUserId = createUserId,

            AmendDate = amendDate,
            AmendUserId = amendUserId,

            RemoveDate = removeDate,
            RemoveUserId = removeUserId
        };
    }
}