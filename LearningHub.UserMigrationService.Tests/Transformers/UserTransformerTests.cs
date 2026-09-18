using LearningHub.UserMigrationService.Models.Extraction;
using LearningHub.UserMigrationService.Transformers;

namespace LearningHub.UserMigrationService.Tests.Transformers;

public class UserTransformerTests
{
    [Fact]
    public void Transform_MapsUserFields()
    {
        var migrationRunId = Guid.NewGuid();

        var source = new ElfhUser
        {
            UserId = 123,
            FirstName = " John ",
            LastName = " Smith ",
            EmailAddress = " john.smith@test.com ",
            AltEmailAddress = " alt@test.com ",
            UserName = "jsmith",
            RegistrationCode = "REG123",
            Active = true,
            PasswordHash = "HASH",
            MustChangeNextLogin = true,
            PasswordLifeCounter = 10,
            RemoteLoginKey = "KEY",
            RestrictToSSO = true,
            Deleted = false,
            AmendUserId = 99,
            CreatedDate = new DateTime(2025, 1, 1),
            AmendDate = new DateTime(2025, 2, 1)
        };

        var transformer = new UserTransformer();

        var result =
            transformer.Transform(
                source,
                migrationRunId);

        Assert.Equal(
            migrationRunId,
            result.MigrationRunId);

        Assert.Equal(
            123,
            result.LegacyUserId);

        Assert.Equal(
            "John",
            result.FirstName);

        Assert.Equal(
            "Smith",
            result.LastName);

        Assert.Equal(
            "john.smith@test.com",
            result.EmailAddress);

        Assert.True(result.IsActive);
        Assert.False(result.IsRemoved);

        Assert.True(
            result.MustChangeNextLogin);

        Assert.True(
            result.RestrictToSso);

        Assert.Equal(
            99,
            result.LegacyAmendUserId);
    }

    [Fact]
    public void Transform_MapsDeletedUser()
    {
        var source = new ElfhUser
        {
            UserId = 123,
            Deleted = true
        };

        var transformer = new UserTransformer();

        var result =
            transformer.Transform(
                source,
                Guid.NewGuid());

        Assert.True(result.IsRemoved);
    }
}