using Microsoft.Extensions.Logging;
using OptimusFrame.Core.Application.Interfaces;

namespace OptimusFrame.Core.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task NotifyProcessingFailureAsync(
            string videoId,
            string? errorMessage,
            string? correlationId)
        {
            _logger.LogWarning(
                """
            NOTIFICATION SIMULATION TO USER
            VideoId: {VideoId}
            CorrelationId: {CorrelationId}
            Error: {Error}
            """,
                videoId,
                correlationId,
                errorMessage);

            return Task.CompletedTask;
        }
    }
}
