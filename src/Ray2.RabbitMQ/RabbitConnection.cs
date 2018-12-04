using RabbitMQ.Client;
using Ray2.RabbitMQ.Configuration;
using System;

namespace Ray2.RabbitMQ
{
    public class RabbitConnection : IRabbitConnection
    {
        private readonly RabbitOptions Options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConnectionFactory _connectionFactory;
        private IConnection connection;
        public RabbitConnection(IServiceProvider serviceProvider, RabbitOptions options)
        {
            this._serviceProvider = serviceProvider;
            this.Options = options;
            this._connectionFactory = new ConnectionFactory
            {
                UserName = this.Options.UserName,
                Password = this.Options.Password,
                VirtualHost = this.Options.VirtualHost,
                AutomaticRecoveryEnabled = false
            };
            this.connection = this._connectionFactory.CreateConnection(this.Options.EndPoints);
        }

        public void Close()
        {
            this.Close();
        }

        public IRabbitChannel CreateChannel()
        {
            if (!this.connection.IsOpen)
            {
                throw new Exception("rabbitMQ service has been disconnected");
            }
            IModel model = this.connection.CreateModel();
            return new RabbitChannel(model,this);
        }
    }
}
