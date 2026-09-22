using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Services;

public static class TransformationValidator
{
    public static IReadOnlyList<string> Validate(
        TransformedUser user)
    {
        var errors = new List<string>();

        if (user.LegacyUserId <= 0)
        {
            errors.Add(
                "LegacyUserId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(user.EmailAddress) &&
            string.IsNullOrWhiteSpace(user.UserName))
        {
            errors.Add(
                "User must have either EmailAddress or UserName.");
        }

        return errors;
    }

    public static IReadOnlyList<string> Validate(
        TransformedOrganisation organisation)
    {
        var errors = new List<string>();

        if (organisation.LegacyOrganisationId <= 0)
        {
            errors.Add(
                "LegacyOrganisationId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(
                organisation.LegacyOrganisationName))
        {
            errors.Add(
                "Organisation name is required.");
        }

        return errors;
    }

    public static IReadOnlyList<string> Validate(
        TransformedProfessionalBody professionalBody)
    {
        var errors = new List<string>();

        if (professionalBody.LegacyProfessionalBodyId <= 0)
        {
            errors.Add(
                "LegacyProfessionalBodyId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(
                professionalBody.LegacyProfessionalBody))
        {
            errors.Add(
                "Professional body name is required.");
        }

        return errors;
    }

    public static IReadOnlyList<string> Validate(
        TransformedOrganisationType organisationType)
    {
        var errors = new List<string>();

        if (organisationType.LegacyOrganisationTypeId <= 0)
        {
            errors.Add(
                "LegacyOrganisationTypeId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(
                organisationType.LegacyOrganisationType))
        {
            errors.Add(
                "Organisation type name is required.");
        }

        return errors;
    }
}