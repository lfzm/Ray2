using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Ray2.EventSource;
using Ray2.MQ;
using Ray2.RabbitMQ.Configuration;
using Ray2.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ
{
    public class EventPublisher : IEventPublisher
    {
        private readonly ConcurrentQueue<IRabbitProducer> ProducersPool = new ConcurrentQueue<IRabbitProducer>();
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitOptions _options;
        private readonly ILogger _logger;
        private readonly ISerializer _serializer;
        private readonly string providerName;
        private int MaxChannelCount = 0;
        public EventPublisher(IServiceProvider serviceProvider, string providerName)
        {
            this.providerName = providerName;
            this._serviceProvider = serviceProvider;
            this._logger = this._serviceProvider.GetRequiredService<ILogger<EventPublisher>>();
            this._options = serviceProvider.GetRequiredService<IOptionsSnapshot<RabbitOptions>>().Get(providerName);
            this._serializer = this._serviceProvider.GetRequiredServiceByName<ISerializer>(_options.SerializationType);
            this.MaxChannelCount = this._options.ConnectionPoolCount * 20;//20 channels per connection pool
        }

        public async Task<bool> Publish(string topic, EventModel model)
        {
            IRabbitProducer producer = await this.GetProducer();
            return await this.Publish(topic, model, producer);
        }
        public async Task<bool> Publish(string topic, IList<EventModel> models)
        {
            IRabbitProducer producer = await this.GetProducer();
            foreach (var model in models)
            {
                await this.Publish(topic, model, producer);
            }
            return true;
        }
        public Task<bool> Publish(string topic, EventModel model, IRabbitProducer producer)
        {
            var message = new PublishMessage(model, this._serializer);
            return producer.Publish(topic, topic, message);
        }

        public async Task<IRabbitProducer> GetProducer()
        {
            IRabbitProducer producer;
            if (ProducersPool.TryDequeue(out producer))
            {
                if (producer.IsAvailable())
                {
                    ProducersPool.Enqueue(producer);
                    return producer;
                }
            }
            else
            {
                if (Interlocked.Decrement(ref this.MaxChannelCount) > 0)
                {
                    producer = new RabbitProducer(this._serviceProvider, this.providerName, this._serializer);
                    ProducersPool.Enqueue(producer);
                    return producer;
                }
                else
                {
                    await Task.Delay(500);
                }
            }
            return await this.GetProducer();
        }
    }
}
