using OptimusFrame.Core.Application.Events;
using OptimusFrame.Core.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OptimusFrame.Core.Infrastructure.Messaging
{
    public class VideoPublisher : IVideoEventPublisher
    {
        private readonly RabbitMqConnection _connection;

        public VideoPublisher(RabbitMqConnection connection)
        {
            _connection = connection;
        }

        public async Task Publish(VideoProcessingMessage evt)
        {
            await using var connection = await _connection.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "video.processing",
                type: ExchangeType.Direct,
                durable: true
            );

            var message = JsonSerializer.Serialize(evt);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await channel.BasicPublishAsync(
                exchange: "video.processing",
                routingKey: "video.processar",
                mandatory: false,
                basicProperties: properties,
                body: body
            );
        }
    }
}
