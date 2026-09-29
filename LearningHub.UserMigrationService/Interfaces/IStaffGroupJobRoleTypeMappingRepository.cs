using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IStaffGroupJobRoleTypeMappingRepository
{
    Task<IReadOnlyList<StaffGroupJobRoleTypeMapping>>
        GetMappingsAsync(
            CancellationToken cancellationToken = default);
}