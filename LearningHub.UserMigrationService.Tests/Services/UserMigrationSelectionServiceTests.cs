using LearningHub.UserMigrationService.Configuration;
using LearningHub.UserMigrationService.Interfaces;
using LearningHub.UserMigrationService.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace LearningHub.UserMigrationService.Tests.Services;

public class UserMigrationSelectionServiceTests
{
    private readonly Mock<ILegacyRepository> _legacyRepository;
    private readonly Mock<ILearningHubRepository> _learningHubRepository;

    private readonly UserMigrationSelectionService _service;


    public UserMigrationSelectionServiceTests()
    {
        _legacyRepository = new Mock<ILegacyRepository>();
        _learningHubRepository = new Mock<ILearningHubRepository>();
        var migrationOptions = Options.Create(new MigrationOptions
        {
            UserMigrationMonths = 24
        });
        _service = new UserMigrationSelectionService(
            _legacyRepository.Object,
            _learningHubRepository.Object,
              migrationOptions);
    }

    [Fact]
    public async Task PopulateUsersToMigrateAsync_ShouldMergeElfhAndLearningHubUsers()
    {
        // Arrange

        var elfhUsers = new List<int>
        {
            1001,
            1002,
            1003
        };

        var learningHubUsers = new List<int>
        {
            1003,
            1004,
            1005
        };

        _legacyRepository
            .Setup(x => x.GetElfhUsersToMigrateAsync(
                24,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(elfhUsers);

        _learningHubRepository
            .Setup(x => x.GetUsersWithExistingDataAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(learningHubUsers);

        _learningHubRepository
            .Setup(x => x.InsertUsersToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var result =
            await _service.PopulateUsersToMigrateAsync(
                CancellationToken.None);

        // Assert

        Assert.Equal(5, result);

        _learningHubRepository.Verify(
            x => x.InsertUsersToMigrateAsync(
                It.Is<IEnumerable<int>>(ids =>
                    ids.OrderBy(x => x).SequenceEqual(
                        new[]
                        {
                            1001,
                            1002,
                            1003,
                            1004,
                            1005
                        })),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PopulateUsersToMigrateAsync_ShouldRemoveDuplicateUsers()
    {
        // Arrange

        var elfhUsers = new List<int>
        {
            1001,
            1002,
            1003
        };

        var learningHubUsers = new List<int>
        {
            1002,
            1003,
            1004
        };

        _legacyRepository
            .Setup(x => x.GetElfhUsersToMigrateAsync(
                24,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(elfhUsers);

        _learningHubRepository
            .Setup(x => x.GetUsersWithExistingDataAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(learningHubUsers);

        _learningHubRepository
            .Setup(x => x.InsertUsersToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var result =
            await _service.PopulateUsersToMigrateAsync(
                CancellationToken.None);

        // Assert

        Assert.Equal(4, result);

        _learningHubRepository.Verify(
            x => x.InsertUsersToMigrateAsync(
                It.Is<IEnumerable<int>>(ids =>
                    ids.Distinct().Count() == 4),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PopulateUsersToMigrateAsync_ShouldReturnZero_WhenNoUsersFound()
    {
        // Arrange

        _legacyRepository
            .Setup(x => x.GetElfhUsersToMigrateAsync(
                24,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        _learningHubRepository
            .Setup(x => x.GetUsersWithExistingDataAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        _learningHubRepository
            .Setup(x => x.InsertUsersToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        var result =
            await _service.PopulateUsersToMigrateAsync(
                CancellationToken.None);

        // Assert

        Assert.Equal(0, result);

        _learningHubRepository.Verify(
            x => x.InsertUsersToMigrateAsync(
                It.Is<IEnumerable<int>>(ids => !ids.Any()),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PopulateUsersToMigrateAsync_ShouldUse24MonthsForElfhSelection()
    {
        // Arrange

        _legacyRepository
            .Setup(x => x.GetElfhUsersToMigrateAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        _learningHubRepository
            .Setup(x => x.GetUsersWithExistingDataAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int>());

        _learningHubRepository
            .Setup(x => x.InsertUsersToMigrateAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act

        await _service.PopulateUsersToMigrateAsync(
            CancellationToken.None);

        // Assert

        _legacyRepository.Verify(
            x => x.GetElfhUsersToMigrateAsync(
                24,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}