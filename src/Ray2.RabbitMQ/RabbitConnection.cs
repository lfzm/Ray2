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
        public IConnection Connection { get;  }
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
            this.Connection = this._connectionFactory.CreateConnection(this.Options.EndPoints);
        }

        public void Close()
        {
            this.Connection.Close();
            this.Connection.Dispose();
        }

        public IRabbitChannel CreateChannel()
        {
            if (!this.Connection.IsOpen)
            {
                throw new Exception("rabbitMQ service has been disconnected");
            }
            return new RabbitChannel(this);
        }

        public bool IsOpen()
        {
            return this.Connection.IsOpen;
        }
    }
}
