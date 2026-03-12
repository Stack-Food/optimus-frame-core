using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OptimusFrame.Core.Infrastructure.Services;

namespace OptimusFrame.Core.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly Mock<IAmazonSimpleEmailService> _mockSesClient;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _mockSesClient = new Mock<IAmazonSimpleEmailService>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration
            .Setup(c => c["AWS:SES:FromEmail"])
            .Returns("noreply@test.com");

        _mockConfiguration
            .Setup(c => c["AWS:SES:FromName"])
            .Returns("Test Service");

        _service = new NotificationService(
            _mockLogger.Object,
            _mockSesClient.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task NotifyProcessingFailureAsync_ShouldSendEmail_WhenValidDataProvided()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var errorMessage = "Processing failed";
        var correlationId = Guid.NewGuid().ToString();

        var response = new SendEmailResponse
        {
            MessageId = "ses-message-id-123"
        };

        _mockSesClient
            .Setup(s => s.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _service.NotifyProcessingFailureAsync(videoId, errorMessage, correlationId);

        // Assert
        _mockSesClient.Verify(
            s => s.SendEmailAsync(
                It.Is<SendEmailRequest>(req =>
                    req.Source == "Test Service <noreply@test.com>" &&
                    req.Destination.ToAddresses.Count == 1 &&
                    req.Message.Subject.Data.Contains(videoId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyProcessingFailureAsync_ShouldIncludeErrorDetailsInEmail()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var errorMessage = "File format not supported";
        var correlationId = Guid.NewGuid().ToString();

        var response = new SendEmailResponse
        {
            MessageId = "ses-message-id-123"
        };

        SendEmailRequest? capturedRequest = null;

        _mockSesClient
            .Setup(s => s.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response)
            .Callback<SendEmailRequest, CancellationToken>((req, _) => capturedRequest = req);

        // Act
        await _service.NotifyProcessingFailureAsync(videoId, errorMessage, correlationId);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Message.Body.Html.Data.Should().Contain(videoId);
        capturedRequest.Message.Body.Html.Data.Should().Contain(errorMessage);
        capturedRequest.Message.Body.Html.Data.Should().Contain(correlationId);
        capturedRequest.Message.Body.Text.Data.Should().Contain(videoId);
        capturedRequest.Message.Body.Text.Data.Should().Contain(errorMessage);
        capturedRequest.Message.Body.Text.Data.Should().Contain(correlationId);
    }

    [Fact]
    public async Task NotifyProcessingFailureAsync_ShouldLogWarning_BeforeSendingEmail()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var errorMessage = "Processing failed";
        var correlationId = Guid.NewGuid().ToString();

        var response = new SendEmailResponse
        {
            MessageId = "ses-message-id-123"
        };

        _mockSesClient
            .Setup(s => s.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _service.NotifyProcessingFailureAsync(videoId, errorMessage, correlationId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Sending failure notification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyProcessingFailureAsync_ShouldLogSuccess_WhenEmailSentSuccessfully()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var errorMessage = "Processing failed";
        var correlationId = Guid.NewGuid().ToString();

        var response = new SendEmailResponse
        {
            MessageId = "ses-message-id-123"
        };

        _mockSesClient
            .Setup(s => s.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _service.NotifyProcessingFailureAsync(videoId, errorMessage, correlationId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Email notification sent successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyProcessingFailureAsync_ShouldHandleException_WhenSesClientFails()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var errorMessage = "Processing failed";
        var correlationId = Guid.NewGuid().ToString();

        _mockSesClient
            .Setup(s => s.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SES service unavailable"));

        // Act
        Func<Task> act = async () => await _service.NotifyProcessingFailureAsync(videoId, errorMessage, correlationId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotifyProcessingFailureAsync_ShouldLogError_WhenSesClientFails()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var errorMessage = "Processing failed";
        var correlationId = Guid.NewGuid().ToString();

        _mockSesClient
            .Setup(s => s.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SES service unavailable"));

        // Act
        await _service.NotifyProcessingFailureAsync(videoId, errorMessage, correlationId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send email notification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyProcessingFailureAsync_ShouldLogFallbackNotification_WhenSesClientFails()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var errorMessage = "Processing failed";
        var correlationId = Guid.NewGuid().ToString();

        _mockSesClient
            .Setup(s => s.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SES service unavailable"));

        // Act
        await _service.NotifyProcessingFailureAsync(videoId, errorMessage, correlationId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("NOTIFICATION (EMAIL FAILED - LOG ONLY)")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyProcessingFailureAsync_ShouldUseDefaultValues_WhenConfigurationMissing()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AWS:SES:FromEmail"]).Returns((string?)null);
        mockConfig.Setup(c => c["AWS:SES:FromName"]).Returns((string?)null);

        var service = new NotificationService(
            _mockLogger.Object,
            _mockSesClient.Object,
            mockConfig.Object);

        var videoId = Guid.NewGuid().ToString();
        var errorMessage = "Processing failed";
        var correlationId = Guid.NewGuid().ToString();

        var response = new SendEmailResponse
        {
            MessageId = "ses-message-id-123"
        };

        SendEmailRequest? capturedRequest = null;

        _mockSesClient
            .Setup(s => s.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response)
            .Callback<SendEmailRequest, CancellationToken>((req, _) => capturedRequest = req);

        // Act
        await service.NotifyProcessingFailureAsync(videoId, errorMessage, correlationId);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Source.Should().Be("OptimusFrame <noreply@optimusframe.com>");
    }

    [Fact]
    public async Task NotifyProcessingFailureAsync_ShouldHandleNullErrorMessage()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        string? errorMessage = null;
        var correlationId = Guid.NewGuid().ToString();

        var response = new SendEmailResponse
        {
            MessageId = "ses-message-id-123"
        };

        _mockSesClient
            .Setup(s => s.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        Func<Task> act = async () => await _service.NotifyProcessingFailureAsync(videoId, errorMessage, correlationId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotifyProcessingFailureAsync_ShouldHandleNullCorrelationId()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var errorMessage = "Processing failed";
        string? correlationId = null;

        var response = new SendEmailResponse
        {
            MessageId = "ses-message-id-123"
        };

        _mockSesClient
            .Setup(s => s.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        Func<Task> act = async () => await _service.NotifyProcessingFailureAsync(videoId, errorMessage, correlationId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NotifyProcessingFailureAsync_ShouldSendToCorrectRecipient()
    {
        // Arrange
        var videoId = Guid.NewGuid().ToString();
        var errorMessage = "Processing failed";
        var correlationId = Guid.NewGuid().ToString();

        var response = new SendEmailResponse
        {
            MessageId = "ses-message-id-123"
        };

        SendEmailRequest? capturedRequest = null;

        _mockSesClient
            .Setup(s => s.SendEmailAsync(
                It.IsAny<SendEmailRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response)
            .Callback<SendEmailRequest, CancellationToken>((req, _) => capturedRequest = req);

        // Act
        await _service.NotifyProcessingFailureAsync(videoId, errorMessage, correlationId);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Destination.ToAddresses.Should().HaveCount(1);
        capturedRequest.Destination.ToAddresses[0].Should().Be($"user-{videoId}@example.com");
    }
}
