using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OptimusFrame.Core.Domain.Entities;
using OptimusFrame.Core.Domain.Enums;
using OptimusFrame.Core.Infrastructure.Data;
using OptimusFrame.Core.Infrastructure.Repositories;

namespace OptimusFrame.Core.Tests.Repositories;

public class MediaRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly MediaRepository _repository;

    public MediaRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new MediaRepository(_context);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateMedia_WhenValidDataProvided()
    {
        // Arrange
        var media = new Media
        {
            UserName = "test@example.com",
            FileName = "test.mp4",
            Base64 = "base64data",
            UrlBucket = "s3://bucket/test.mp4",
            Status = MediaStatus.Uploaded,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(media);

        // Assert
        result.Should().NotBeNull();
        result.MediaId.Should().NotBeEmpty();
        result.UserName.Should().Be("test@example.com");
        result.FileName.Should().Be("test.mp4");

        var savedMedia = await _context.Media.FirstOrDefaultAsync(m => m.MediaId == result.MediaId);
        savedMedia.Should().NotBeNull();
        savedMedia!.UserName.Should().Be("test@example.com");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenMediaIsNull()
    {
        // Act
        Func<Task> act = async () => await _repository.CreateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetByUserNameAsync_ShouldReturnUserVideos_WhenVideosExist()
    {
        // Arrange
        var userName = "test@example.com";
        var media1 = new Media
        {
            MediaId = Guid.NewGuid(),
            UserName = userName,
            FileName = "video1.mp4",
            Base64 = "data1",
            UrlBucket = "s3://bucket/video1.mp4",
            Status = MediaStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var media2 = new Media
        {
            MediaId = Guid.NewGuid(),
            UserName = userName,
            FileName = "video2.mp4",
            Base64 = "data2",
            UrlBucket = "s3://bucket/video2.mp4",
            Status = MediaStatus.Process,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await _context.Media.AddRangeAsync(media1, media2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserNameAsync(userName);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().FileName.Should().Be("video2.mp4"); // Mais recente primeiro
        result.Last().FileName.Should().Be("video1.mp4");
    }

    [Fact]
    public async Task GetByUserNameAsync_ShouldReturnEmpty_WhenNoVideosExist()
    {
        // Act
        var result = await _repository.GetByUserNameAsync("nonexistent@example.com");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByUserNameAsync_ShouldThrowException_WhenUserNameIsNull()
    {
        // Act
        Func<Task> act = async () => await _repository.GetByUserNameAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMedia_WhenMediaExists()
    {
        // Arrange
        var mediaId = Guid.NewGuid();
        var media = new Media
        {
            MediaId = mediaId,
            UserName = "test@example.com",
            FileName = "test.mp4",
            Base64 = "data",
            UrlBucket = "s3://bucket/test.mp4",
            Status = MediaStatus.Uploaded,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Media.AddAsync(media);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(mediaId);

        // Assert
        result.Should().NotBeNull();
        result!.MediaId.Should().Be(mediaId);
        result.FileName.Should().Be("test.mp4");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMediaNotFound()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateToCompleted_WhenSuccessful()
    {
        // Arrange
        var mediaId = Guid.NewGuid();
        var media = new Media
        {
            MediaId = mediaId,
            UserName = "test@example.com",
            FileName = "test.mp4",
            Base64 = "data",
            UrlBucket = "s3://bucket/test.mp4",
            Status = MediaStatus.Process,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Media.AddAsync(media);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateStatusAsync(
            mediaId,
            MediaStatus.Completed,
            "s3://bucket/output.zip",
            null);

        // Assert
        var updated = await _context.Media.FindAsync(mediaId);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be(MediaStatus.Completed);
        updated.OutputUri.Should().Be("s3://bucket/output.zip");
        updated.CompletedAt.Should().NotBeNull();
        updated.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateToFailed_WhenError()
    {
        // Arrange
        var mediaId = Guid.NewGuid();
        var media = new Media
        {
            MediaId = mediaId,
            UserName = "test@example.com",
            FileName = "test.mp4",
            Base64 = "data",
            UrlBucket = "s3://bucket/test.mp4",
            Status = MediaStatus.Process,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Media.AddAsync(media);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateStatusAsync(
            mediaId,
            MediaStatus.Failed,
            null,
            "Processing error occurred");

        // Assert
        var updated = await _context.Media.FindAsync(mediaId);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be(MediaStatus.Failed);
        updated.ErrorMessage.Should().Be("Processing error occurred");
        updated.CompletedAt.Should().NotBeNull();
        updated.OutputUri.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrowException_WhenMediaNotFound()
    {
        // Act
        Func<Task> act = async () => await _repository.UpdateStatusAsync(
            Guid.NewGuid(),
            MediaStatus.Completed,
            "output.zip",
            null);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
