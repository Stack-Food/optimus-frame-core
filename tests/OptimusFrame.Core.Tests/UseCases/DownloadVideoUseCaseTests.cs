using FluentAssertions;
using Moq;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Application.UseCases.DownloadVideo;
using OptimusFrame.Core.Domain.Entities;
using OptimusFrame.Core.Domain.Enums;

namespace OptimusFrame.Core.Tests.UseCases;

public class DownloadVideoUseCaseTests
{
    private readonly Mock<IMediaRepository> _mediaRepositoryMock;
    private readonly Mock<IMediaService> _mediaServiceMock;
    private readonly DownloadVideoUseCase _useCase;

    public DownloadVideoUseCaseTests()
    {
        _mediaRepositoryMock = new Mock<IMediaRepository>();
        _mediaServiceMock = new Mock<IMediaService>();
        _useCase = new DownloadVideoUseCase(
            _mediaRepositoryMock.Object,
            _mediaServiceMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldReturnDownloadUrl_WhenVideoExists()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        var outputUri = "s3://bucket/output/video_frames.zip";
        var expectedUrl = "https://s3.amazonaws.com/presigned-url";

        var media = new Media
        {
            MediaId = videoId,
            UserName = "test@example.com",
            FileName = "video.mp4",
            Status = MediaStatus.Completed,
            OutputUri = outputUri,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(videoId))
            .ReturnsAsync(media);

        _mediaServiceMock
            .Setup(x => x.GenerateDownloadUrlAsync(outputUri, 60))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _useCase.Execute(videoId);

        // Assert
        result.Should().Be(expectedUrl);
        _mediaRepositoryMock.Verify(x => x.GetByIdAsync(videoId), Times.Once);
        _mediaServiceMock.Verify(x => x.GenerateDownloadUrlAsync(outputUri, 60), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenVideoNotFound()
    {
        // Arrange
        var videoId = Guid.NewGuid();

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(videoId))
            .ReturnsAsync((Media?)null);

        // Act
        Func<Task> act = async () => await _useCase.Execute(videoId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Video with ID {videoId} not found");
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenOutputUriIsNull()
    {
        // Arrange
        var videoId = Guid.NewGuid();

        var media = new Media
        {
            MediaId = videoId,
            UserName = "test@example.com",
            FileName = "video.mp4",
            Status = MediaStatus.Process,
            OutputUri = null,
            CreatedAt = DateTime.UtcNow
        };

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(videoId))
            .ReturnsAsync(media);

        // Act
        Func<Task> act = async () => await _useCase.Execute(videoId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Video {videoId} has not been processed yet");
    }

    [Fact]
    public async Task Execute_ShouldThrowException_WhenOutputUriIsEmpty()
    {
        // Arrange
        var videoId = Guid.NewGuid();

        var media = new Media
        {
            MediaId = videoId,
            UserName = "test@example.com",
            FileName = "video.mp4",
            Status = MediaStatus.Process,
            OutputUri = "",
            CreatedAt = DateTime.UtcNow
        };

        _mediaRepositoryMock
            .Setup(x => x.GetByIdAsync(videoId))
            .ReturnsAsync(media);

        // Act
        Func<Task> act = async () => await _useCase.Execute(videoId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Video {videoId} has not been processed yet");
    }
}
