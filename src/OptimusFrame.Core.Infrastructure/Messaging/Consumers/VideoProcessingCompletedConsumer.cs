using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OptimusFrame.Core.Application.Events;
using OptimusFrame.Core.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OptimusFrame.Core.Infrastructure.Messaging.Consumers
{
    public class VideoProcessingCompletedConsumer : BackgroundService
    {
        private readonly ILogger<VideoProcessingCompletedConsumer> _logger;
        private readonly RabbitMqConnection _rabbitMqConnection;
        private IConnection? _connection;
        private IChannel? _channel;

        private const string QueueName = "video-processing-completed-queue";

        private readonly IServiceScopeFactory _scopeFactory;

        public VideoProcessingCompletedConsumer(
            ILogger<VideoProcessingCompletedConsumer> logger,
            RabbitMqConnection rabbitMqConnection,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _rabbitMqConnection = rabbitMqConnection;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = await _rabbitMqConnection.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += HandleMessageAsync;

            await _channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer);
        }

        private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            var message = JsonSerializer.Deserialize<VideoProcessingCompletedMessage>(json);

            if (message == null)
                return;

            if (!message.Success)
            {
                using var scope = _scopeFactory.CreateScope();

                var notificationService = scope.ServiceProvider
                    .GetRequiredService<INotificationService>();

                _logger.LogError("Video processing failed for video {VideoId}. Notificating user... Error: {ErrorMessage}",
                    message.VideoId, message.ErrorMessage);

                await notificationService.NotifyProcessingFailureAsync(
                    message.VideoId,
                    message.ErrorMessage,
                    message.CorrelationId);
            }

            _logger.LogInformation("Received video processing completed message for video {VideoId} with success status {Success}",
                message.VideoId, message.Success);
        }
    }
}
