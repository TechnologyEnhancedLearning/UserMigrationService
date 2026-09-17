namespace LearningHub.UserMigrationService.Transformers;

public static class OrganisationRegionResolver
{
    public static string? Resolve(string? postcode)
    {
        if (string.IsNullOrWhiteSpace(postcode))
            return null;

        var normalized =
            postcode
                .Trim()
                .ToUpperInvariant()
                .Replace(" ", string.Empty);

        // Northern Ireland
        if (normalized.StartsWith("BT"))
            return "Northern Ireland";

        // Scotland
        if (IsScottishPostcode(normalized))
            return "Scotland";

        // Wales
        if (IsWelshPostcode(normalized))
            return "Wales";

        // England
        return "England";
    }

    private static bool IsScottishPostcode(string postcode)
    {
        var prefixes = new[]
        {
            "AB",
            "DD",
            "DG",
            "EH",
            "FK",
            "G",
            "HS",
            "IV",
            "KA",
            "KW",
            "KY",
            "ML",
            "PA",
            "PH",
            "ZE"
        };

        return prefixes.Any(postcode.StartsWith);
    }

    private static bool IsWelshPostcode(string postcode)
    {
        var prefixes = new[]
        {
            "CF",
            "LD",
            "LL",
            "NP",
            "SA",
            "SY"
        };

        return prefixes.Any(postcode.StartsWith);
    }
}