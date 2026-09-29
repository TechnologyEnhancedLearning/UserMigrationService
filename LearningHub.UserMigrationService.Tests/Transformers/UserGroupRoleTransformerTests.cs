using LearningHub.UserMigrationService.Transformers;

namespace LearningHub.UserMigrationService.Tests.Transformers;

public class UserGroupRoleTransformerTests
{
    [Fact]
    public void Transform_ShouldMapValues()
    {
        var migrationRunId = Guid.NewGuid();

        var transformer =
            new UserGroupRoleTransformer();

        var result =
            transformer.Transform(
                legacyUserId: 100,
                legacyUserGroupId: 200,
                legacyUserGroupReporterId: 300,
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
            result.LegacyUserGroupId);

        Assert.Equal(
            300,
            result.LegacyUserGroupReporterId);

        Assert.Equal(
            5,
            result.RoleId);

        Assert.False(result.IsRemoved);
    }
}