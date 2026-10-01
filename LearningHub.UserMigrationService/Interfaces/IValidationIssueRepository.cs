using LearningHub.UserMigrationService.Models;

namespace LearningHub.UserMigrationService.Interfaces;

public interface IValidationIssueRepository
{
    Task InsertAsync(IReadOnlyCollection<ValidationIssue> issues,CancellationToken cancellationToken = default);
}