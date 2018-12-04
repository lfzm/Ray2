using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using RabbitMQ.Client;
using Ray2.Serialization;
using System;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ
{
    public class RabbitProducer : IRabbitProducer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        private readonly IRabbitChannel _channel;
        private readonly string providerName;
        public RabbitProducer(IServiceProvider serviceProvider, string providerName, ISerializer serializer)
        {
            this.providerName = providerName;
            this._serviceProvider = serviceProvider;
            this._serializer = serializer;
            this._logger = this._serviceProvider.GetRequiredService<ILogger<RabbitProducer>>();
            var channelFactory = serviceProvider.GetRequiredServiceByName<IRabbitChannelFactory>(providerName);
            this._channel = channelFactory.GetChannel();
        }

        public bool IsAvailable()
        {
            return this._channel.IsOpen();
        }

        public Task<bool> Publish(string exchange, string routingKey, PublishMessage message)
        {
            try
            {
                var data = this._serializer.Serialize(message);
                this._channel.Model.BasicPublish(exchange, routingKey, null, data);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"{providerName}-{exchange}&{routingKey} RabbitMQ publishing failed");
                return Task.FromResult(false);
            }
        }
    }
}
