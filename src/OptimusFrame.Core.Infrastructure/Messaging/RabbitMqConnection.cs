using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace OptimusFrame.Core.Infrastructure.Messaging
{
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
