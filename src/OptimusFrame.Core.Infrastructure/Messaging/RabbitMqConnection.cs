using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace OptimusFrame.Core.Infrastructure.Messaging
{
    public class RabbitMqConnection
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqConnection()
        {
            _factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
        }

        public async Task<IConnection> CreateConnectionAsync()
        {
            return await _factory.CreateConnectionAsync();
        }
    }
}
