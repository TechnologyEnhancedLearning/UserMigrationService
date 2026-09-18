namespace LearningHub.UserMigrationService.Interfaces;

public interface ITransformationValidator<T>
{
    IReadOnlyList<string> Validate(T value);
}