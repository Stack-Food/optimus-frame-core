using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;

namespace OptimusFrame.Core.Infrastructure.Messaging
{
    [ExcludeFromCodeCoverage]
    public class RabbitMqConnection
    {
        private readonly ConnectionFactory _factory;
        private readonly RabbitMqSettings _settings;

        public RabbitMqConnection(IOptions<RabbitMqSettings> settings)
        {
            _settings = settings.Value;
            _factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost
            };
        }

        public async Task<IConnection> CreateConnectionAsync()
        {
            return await _factory.CreateConnectionAsync();
        }
    }
}
