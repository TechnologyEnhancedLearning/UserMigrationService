namespace LearningHub.UserMigrationService.Transformers;

public static class TransformationValueHelper
{
    public static string? NormalizeString(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    public static bool ToBoolean(
        object? value,
        bool defaultValue = false)
    {
        if (value is null || value == DBNull.Value)
        {
            return defaultValue;
        }

        return value switch
        {
            bool boolean => boolean,

            byte number =>
                number != 0,

            short number =>
                number != 0,

            int number =>
                number != 0,

            long number =>
                number != 0,

            string text when
                bool.TryParse(text, out var result) =>
                result,

            string text when
                int.TryParse(text, out var number) =>
                number != 0,

            _ => defaultValue
        };
    }

    public static DateTimeOffset? ToUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return new DateTimeOffset(
            DateTime.SpecifyKind(
                value.Value,
                DateTimeKind.Utc));
    }

    public static DateTimeOffset? ToUtc(
        DateTimeOffset? value)
    {
        return value?.ToUniversalTime();
    }
}