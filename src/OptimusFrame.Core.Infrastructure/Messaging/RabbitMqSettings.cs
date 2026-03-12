using System.Diagnostics.CodeAnalysis;

namespace OptimusFrame.Core.Infrastructure.Messaging
{
    [ExcludeFromCodeCoverage]
    public class RabbitMqSettings
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public QueueSettings Queues { get; set; } = new QueueSettings();
    }

    [ExcludeFromCodeCoverage]
    public class QueueSettings
    {
        public string VideoProcessing { get; set; } = "video.processing.input";
        public string VideoCompleted { get; set; } = "video.processing.completed";
    }
}
