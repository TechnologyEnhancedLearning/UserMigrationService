using LearningHub.UserMigrationService.Transformers;

namespace LearningHub.UserMigrationService.Tests.Transformers;

public class UserOrganisationTransformerTests
{
    [Fact]
    public void Transform_ShouldMapValues()
    {
        var migrationRunId = Guid.NewGuid();

        var transformer =
            new UserOrganisationTransformer();

        var result = transformer.Transform(
      elfhUserId: 100,
      elfhLocationId: 200,
      userId: 100,
      organisationId: 200,
      jobRoleTypeId: 5,
      jobRole: "Test Role",
      startDate: new DateTimeOffset(
          2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
      endDate: null,
      createDate: new DateTimeOffset(
          2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
      createUserId: null,
      amendDate: null,
      amendUserId: null,
      removeDate: null,
      removeUserId: null,
      migrationRunId: migrationRunId);

        Assert.Equal(migrationRunId, result.MigrationRunId);
        Assert.Equal(100, result.ElfhUserId);
        Assert.Equal(200, result.ElfhLocationId);
        Assert.Equal(100, result.UserId);
        Assert.Equal(200, result.OrganisationId);
        Assert.Equal(5, result.JobRoleTypeId);
        Assert.Equal("Test Role", result.JobRole);
    }
}