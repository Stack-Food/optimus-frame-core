using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using OptimusFrame.Core.Application.DTOs.Request;
using OptimusFrame.Core.Application.Events;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Application.UseCases.UploadMedia;
using OptimusFrame.Core.Domain.Entities;
using OptimusFrame.Core.Domain.Enums;

namespace OptimusFrame.Core.Tests.UseCases;

public class UploadMediaUseCaseTests
{
    private readonly Mock<IMediaRepository> _mockMediaRepository;
    private readonly Mock<IMediaService> _mockMediaService;
    private readonly Mock<IVideoEventPublisher> _mockPublisher;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly UploadMediaUseCase _useCase;

    public UploadMediaUseCaseTests()
    {
        _mockMediaRepository = new Mock<IMediaRepository>();
        _mockMediaService = new Mock<IMediaService>();
        _mockPublisher = new Mock<IVideoEventPublisher>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration
            .Setup(c => c["AWS:S3:BucketName"])
            .Returns("test-bucket");

        _useCase = new UploadMediaUseCase(
            _mockMediaRepository.Object,
            _mockMediaService.Object,
            _mockPublisher.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task UploadVideoToS3_ShouldCreateMedia_WhenValidDataProvided()
    {
        // Arrange
        var videoBytes = new byte[] { 1, 2, 3, 4, 5 };
        var request = new UploadVideoBase64Request
        {
            UserName = "test@example.com",
            FileName = "test.mp4"
        };

        var createdMedia = new Media
        {
            MediaId = Guid.NewGuid(),
            UserName = request.UserName,
            Status = MediaStatus.Uploaded,
            CreatedAt = DateTime.UtcNow,
            FileName = request.FileName
        };

        _mockMediaRepository
            .Setup(r => r.CreateAsync(It.IsAny<Media>()))
            .ReturnsAsync(createdMedia);

        _mockMediaService
            .Setup(s => s.UploadVideoAsync(
                It.IsAny<byte[]>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync("s3://test-bucket/video.mp4");

        // Act
        await _useCase.UploadVideoToS3(videoBytes, request);

        // Assert
        _mockMediaRepository.Verify(
            r => r.CreateAsync(It.Is<Media>(m =>
                m.UserName == request.UserName &&
                m.Status == MediaStatus.Uploaded)),
            Times.Once);
    }

    [Fact]
    public async Task UploadVideoToS3_ShouldCallMediaService_WithCorrectParameters()
    {
        // Arrange
        var videoBytes = new byte[] { 1, 2, 3, 4, 5 };
        var request = new UploadVideoBase64Request
        {
            UserName = "test@example.com",
            FileName = "test.mp4"
        };

        var mediaId = Guid.NewGuid();
        var createdMedia = new Media
        {
            MediaId = mediaId,
            UserName = request.UserName,
            Status = MediaStatus.Uploaded,
            CreatedAt = DateTime.UtcNow,
            FileName = request.FileName
        };

        _mockMediaRepository
            .Setup(r => r.CreateAsync(It.IsAny<Media>()))
            .ReturnsAsync(createdMedia);

        _mockMediaService
            .Setup(s => s.UploadVideoAsync(
                It.IsAny<byte[]>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync("video.mp4");

        // Act
        await _useCase.UploadVideoToS3(videoBytes, request);

        // Assert
        _mockMediaService.Verify(
            s => s.UploadVideoAsync(
                videoBytes,
                mediaId,
                request.UserName,
                "optimus-frame-core-bucket"),
            Times.Once);
    }

    [Fact]
    public async Task UploadVideoToS3_ShouldPublishEvent_WhenUploadSucceeds()
    {
        // Arrange
        var videoBytes = new byte[] { 1, 2, 3, 4, 5 };
        var request = new UploadVideoBase64Request
        {
            UserName = "test@example.com",
            FileName = "test.mp4"
        };

        var mediaId = Guid.NewGuid();
        var createdMedia = new Media
        {
            MediaId = mediaId,
            UserName = request.UserName,
            Status = MediaStatus.Uploaded,
            CreatedAt = DateTime.UtcNow,
            FileName = request.FileName
        };

        _mockMediaRepository
            .Setup(r => r.CreateAsync(It.IsAny<Media>()))
            .ReturnsAsync(createdMedia);

        _mockMediaService
            .Setup(s => s.UploadVideoAsync(
                It.IsAny<byte[]>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync("s3://test-bucket/video.mp4");

        // Act
        await _useCase.UploadVideoToS3(videoBytes, request);

        // Assert
        _mockPublisher.Verify(
            p => p.Publish(It.Is<VideoProcessingMessage>(m =>
                m.VideoId == mediaId.ToString() &&
                m.FileName == request.FileName &&
                !string.IsNullOrEmpty(m.CorrelationId))),
            Times.Once);
    }

    [Fact]
    public async Task UploadVideoToS3_ShouldExecuteAllSteps_InCorrectOrder()
    {
        // Arrange
        var videoBytes = new byte[] { 1, 2, 3, 4, 5 };
        var request = new UploadVideoBase64Request
        {
            UserName = "test@example.com",
            FileName = "test.mp4"
        };

        var mediaId = Guid.NewGuid();
        var createdMedia = new Media
        {
            MediaId = mediaId,
            UserName = request.UserName,
            Status = MediaStatus.Uploaded,
            CreatedAt = DateTime.UtcNow,
            FileName = request.FileName
        };

        var callOrder = new List<string>();

        _mockMediaRepository
            .Setup(r => r.CreateAsync(It.IsAny<Media>()))
            .ReturnsAsync(createdMedia)
            .Callback(() => callOrder.Add("CreateMedia"));

        _mockMediaService
            .Setup(s => s.UploadVideoAsync(
                It.IsAny<byte[]>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync("s3://test-bucket/video.mp4")
            .Callback(() => callOrder.Add("UploadVideo"));

        _mockPublisher
            .Setup(p => p.Publish(It.IsAny<VideoProcessingMessage>()))
            .Returns(Task.CompletedTask)
            .Callback(() => callOrder.Add("PublishEvent"));

        // Act
        await _useCase.UploadVideoToS3(videoBytes, request);

        // Assert
        callOrder.Should().HaveCount(3);
        callOrder[0].Should().Be("CreateMedia");
        callOrder[1].Should().Be("UploadVideo");
        callOrder[2].Should().Be("PublishEvent");
    }

    [Fact]
    public async Task UploadVideoToS3_ShouldThrowException_WhenRepositoryFails()
    {
        // Arrange
        var videoBytes = new byte[] { 1, 2, 3, 4, 5 };
        var request = new UploadVideoBase64Request
        {
            UserName = "test@example.com",
            FileName = "test.mp4"
        };

        _mockMediaRepository
            .Setup(r => r.CreateAsync(It.IsAny<Media>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _useCase.UploadVideoToS3(videoBytes, request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task UploadVideoToS3_ShouldThrowException_WhenMediaServiceFails()
    {
        // Arrange
        var videoBytes = new byte[] { 1, 2, 3, 4, 5 };
        var request = new UploadVideoBase64Request
        {
            UserName = "test@example.com",
            FileName = "test.mp4"
        };

        var createdMedia = new Media
        {
            MediaId = Guid.NewGuid(),
            UserName = request.UserName,
            Status = MediaStatus.Uploaded,
            CreatedAt = DateTime.UtcNow,
            FileName = request.FileName
        };

        _mockMediaRepository
            .Setup(r => r.CreateAsync(It.IsAny<Media>()))
            .ReturnsAsync(createdMedia);

        _mockMediaService
            .Setup(s => s.UploadVideoAsync(
                It.IsAny<byte[]>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new Exception("S3 upload failed"));

        // Act
        Func<Task> act = async () => await _useCase.UploadVideoToS3(videoBytes, request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("S3 upload failed");
    }
}
