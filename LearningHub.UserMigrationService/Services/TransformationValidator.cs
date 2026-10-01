using LearningHub.UserMigrationService.Models.Transformation;

namespace LearningHub.UserMigrationService.Services;

public static class TransformationValidator
{
    public static IReadOnlyList<string> Validate(
        TransformedUser user)
    {
        var errors = new List<string>();

        if (user.ElfhUserId <= 0)
        {
            errors.Add(
                "ElfhUserId must be greater than zero.");
        }

        if (user.UserId <= 0)
        {
            errors.Add(
                "UserId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(user.EmailAddress) &&
            string.IsNullOrWhiteSpace(user.ElfhUserName))
        {
            errors.Add(
                "User must have either EmailAddress or ElfhUserName.");
        }

        return errors;
    }

    public static IReadOnlyList<string> Validate(
    TransformedUserOrganisation value)
    {
        var errors = new List<string>();

        if (value.ElfhUserId <= 0)
        {
            errors.Add(
                "ElfhUserId must be greater than zero.");
        }

        if (value.ElfhLocationId <= 0)
        {
            errors.Add(
                "ElfhLocationId must be greater than zero.");
        }

        if (value.UserId <= 0)
        {
            errors.Add(
                "UserId must be greater than zero.");
        }

        if (value.OrganisationId <= 0)
        {
            errors.Add(
                "OrganisationId must be greater than zero.");
        }

        if (value.JobRoleTypeId <= 0)
        {
            errors.Add(
                "JobRoleTypeId must be greater than zero.");
        }

        if (value.StartDate.HasValue &&
            value.EndDate.HasValue &&
            value.EndDate.Value < value.StartDate.Value)
        {
            errors.Add(
                "EndDate cannot be earlier than StartDate.");
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
    public static IReadOnlyList<string> Validate(
    TransformedOrganisation value)
    {
        var errors = new List<string>();

        if (value.ElfhLocationId <= 0)
        {
            errors.Add(
                "ElfhLocationId must be greater than zero.");
        }

        if (value.OrganisationId <= 0)
        {
            errors.Add(
                "OrganisationId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(value.OrganisationName))
        {
            errors.Add(
                "OrganisationName is required.");
        }

        if (value.OrganisationTypeId <= 0)
        {
            errors.Add(
                "OrganisationTypeId must be greater than zero.");
        }

        if (value.CreateDate == default)
        {
            errors.Add(
                "CreateDate is required.");
        }

        if (value.AmendDate.HasValue &&
            value.AmendDate.Value < value.CreateDate)
        {
            errors.Add(
                "AmendDate cannot be earlier than CreateDate.");
        }

        if (value.RemoveDate.HasValue &&
            value.RemoveDate.Value < value.CreateDate)
        {
            errors.Add(
                "RemoveDate cannot be earlier than CreateDate.");
        }

        return errors;
    }
}