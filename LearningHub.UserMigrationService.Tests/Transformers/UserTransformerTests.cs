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
            result.ElfhUserId);

        Assert.Equal(
            123,
            result.UserId);

        Assert.Equal(
            "John",
            result.FirstName);

        Assert.Equal(
            "Smith",
            result.LastName);

        Assert.Equal(
            "john.smith@test.com",
            result.EmailAddress);

        Assert.Equal(
            "jsmith",
            result.ElfhUserName);

        Assert.True(
            result.Active == true);

        Assert.False(
            result.RemoveDate.HasValue);

        Assert.True(
            result.MustChangePassword == true);

        Assert.True(
            result.RestrictToSSO == true);

        Assert.Equal(
            99,
            result.AmendUserId);
    }

    [Fact]
    public void Transform_MapsDeletedUser()
    {
        var source = new ElfhUser
        {
            UserId = 123,
            Deleted = true,
            AmendDate = new DateTime(2025, 2, 1),
            AmendUserId = 99
        };

        var transformer = new UserTransformer();

        var result =
            transformer.Transform(
                source,
                Guid.NewGuid());

        Assert.True(
            result.RemoveDate.HasValue);

        Assert.Equal(
            99,
            result.RemoveUserId);
    }
}