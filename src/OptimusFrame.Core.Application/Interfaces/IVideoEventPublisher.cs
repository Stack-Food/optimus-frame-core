using OptimusFrame.Core.Application.Events;

namespace OptimusFrame.Core.Application.Interfaces
{
    public interface IVideoEventPublisher
    {
        Task Publish(VideoProcessingMessage evt);
    }
}
