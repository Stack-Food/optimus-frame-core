using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OptimusFrame.Core.Application.Events;
using OptimusFrame.Core.Application.Interfaces;
using OptimusFrame.Core.Domain.Enums;
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

        private const string QueueName = "video.processing.completed";

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
            {
                _logger.LogWarning("Received null message from queue");
                await _channel!.BasicAckAsync(args.DeliveryTag, false);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var mediaRepository = scope.ServiceProvider.GetRequiredService<IMediaRepository>();

            try
            {
                if (message.Success)
                {
                    _logger.LogInformation("Video processing completed successfully for video {VideoId}. Updating status...",
                        message.VideoId);

                    await mediaRepository.UpdateStatusAsync(
                        Guid.Parse(message.VideoId),
                        MediaStatus.Completed,
                        message.OutputUri,
                        null);

                    _logger.LogInformation("Status updated to Completed for video {VideoId}", message.VideoId);
                }
                else
                {
                    _logger.LogError("Video processing failed for video {VideoId}. Error: {ErrorMessage}",
                        message.VideoId, message.ErrorMessage);

                    await mediaRepository.UpdateStatusAsync(
                        Guid.Parse(message.VideoId),
                        MediaStatus.Failed,
                        null,
                        message.ErrorMessage);

                    var notificationService = scope.ServiceProvider
                        .GetRequiredService<INotificationService>();

                    await notificationService.NotifyProcessingFailureAsync(
                        message.VideoId,
                        message.ErrorMessage,
                        message.CorrelationId);

                    _logger.LogInformation("Status updated to Failed and user notified for video {VideoId}", message.VideoId);
                }

                await _channel!.BasicAckAsync(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing completed message for video {VideoId}", message.VideoId);
                await _channel!.BasicNackAsync(args.DeliveryTag, false, true);
            }
        }
    }
}
