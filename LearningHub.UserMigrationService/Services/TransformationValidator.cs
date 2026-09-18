namespace LearningHub.UserMigrationService.Services;

public static class TransformationValidator
{
    public static IReadOnlyList<string> ValidateRequired(
        params (string Name, object? Value)[] fields)
    {
        var errors = new List<string>();

        foreach (var field in fields)
        {
            if (field.Value is null)
            {
                errors.Add($"{field.Name} is required.");
                continue;
            }

            if (field.Value is string text &&
                string.IsNullOrWhiteSpace(text))
            {
                errors.Add($"{field.Name} is required.");
            }
        }

        return errors;
    }
}