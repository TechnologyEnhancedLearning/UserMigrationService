using LearningHub.UserMigrationService.Transformers;

namespace LearningHub.UserMigrationService.Tests.Transformers;

public class UserRoleTransformerTests
{
    [Fact]
    public void Transform_ShouldMapValues()
    {
        var migrationRunId = Guid.NewGuid();

        var transformer =
            new UserRoleTransformer();

        var result =
            transformer.Transform(
                legacyUserId: 100,
                legacyAdminLocationId: 200,
                roleId: 5,
                isRemoved: false,
                migrationRunId);

        Assert.Equal(
            migrationRunId,
            result.MigrationRunId);

        Assert.Equal(
            100,
            result.LegacyUserId);

        Assert.Equal(
            200,
            result.LegacyAdminLocationId);

        Assert.Equal(
            5,
            result.RoleId);

        Assert.False(result.IsRemoved);
    }
}