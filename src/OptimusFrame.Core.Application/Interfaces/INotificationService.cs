namespace OptimusFrame.Core.Application.Interfaces
{
    public interface INotificationService
    {
        Task NotifyProcessingFailureAsync(
            string videoId,
            string? errorMessage,
            string? correlationId);
    }
}
