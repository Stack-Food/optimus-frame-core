using FluentAssertions;
using Moq;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Application.UseCases.GetUserVideos;
using OptimusFrame.Core.Domain.Entities;
using OptimusFrame.Core.Domain.Enums;

namespace OptimusFrame.Core.Tests.UseCases;

public class GetUserVideosUseCaseTests
{
    private readonly Mock<IMediaRepository> _mediaRepositoryMock;
    private readonly GetUserVideosUseCase _useCase;

    public GetUserVideosUseCaseTests()
    {
        _mediaRepositoryMock = new Mock<IMediaRepository>();
        _useCase = new GetUserVideosUseCase(_mediaRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldReturnUserVideos_WhenVideosExist()
    {
        // Arrange
        var userName = "test@example.com";
        var videos = new List<Media>
        {
            new Media
            {
                MediaId = Guid.NewGuid(),
                UserName = userName,
                FileName = "video1.mp4",
                Status = MediaStatus.Completed,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                OutputUri = "s3://bucket/output/video1_frames.zip",
                CompletedAt = DateTime.UtcNow
            },
            new Media
            {
                MediaId = Guid.NewGuid(),
                UserName = userName,
                FileName = "video2.mp4",
                Status = MediaStatus.Process,
                CreatedAt = DateTime.UtcNow,
                OutputUri = null,
                CompletedAt = null
            }
        };

        _mediaRepositoryMock
            .Setup(x => x.GetByUserNameAsync(userName))
            .ReturnsAsync(videos);

        // Act
        var result = await _useCase.Execute(userName);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var resultList = result.ToList();
        resultList[0].FileName.Should().Be("video1.mp4");
        resultList[0].Status.Should().Be(MediaStatus.Completed);
        resultList[0].OutputUri.Should().Be("s3://bucket/output/video1_frames.zip");
        resultList[0].CompletedAt.Should().NotBeNull();

        resultList[1].FileName.Should().Be("video2.mp4");
        resultList[1].Status.Should().Be(MediaStatus.Process);
        resultList[1].OutputUri.Should().BeNull();
        resultList[1].CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyList_WhenNoVideosExist()
    {
        // Arrange
        var userName = "test@example.com";
        var emptyList = new List<Media>();

        _mediaRepositoryMock
            .Setup(x => x.GetByUserNameAsync(userName))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _useCase.Execute(userName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_ShouldCallRepository_WithCorrectUserName()
    {
        // Arrange
        var userName = "test@example.com";
        var emptyList = new List<Media>();

        _mediaRepositoryMock
            .Setup(x => x.GetByUserNameAsync(userName))
            .ReturnsAsync(emptyList);

        // Act
        await _useCase.Execute(userName);

        // Assert
        _mediaRepositoryMock.Verify(
            x => x.GetByUserNameAsync(userName),
            Times.Once);
    }
}
