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

        public Task<bool> Publish(string topic, EventModel model)
        {
            IRabbitProducer producer = this.GetProducer();
            return this.Publish(topic, model, producer);
        }
 
        public async Task<bool> Publish(string topic, EventModel model, IRabbitProducer producer)
        {
            var message = new PublishMessage(model, this._serializer);
            bool ret = await producer.Publish(topic, topic, message);
            this.EnqueuePool(producer);
            return ret;
        }

        public IRabbitProducer GetProducer()
        {
            IRabbitProducer producer;
            if (ProducersPool.TryDequeue(out producer))
            {
                if (producer.IsAvailable())
                {
                    return producer;
                }
                else
                {
                    Interlocked.CompareExchange(ref this.MaxChannelCount, 1, 0);
                }
            }
            else
            {
                if (this.MaxChannelCount > 0)
                {
                    Interlocked.Decrement(ref this.MaxChannelCount);
                    producer = new RabbitProducer(this._serviceProvider, this.providerName, this._serializer);
                    return producer;
                }
                else
                {
                    Task.Delay(500).GetAwaiter().GetResult();
                }
            }
            return this.GetProducer();
        }

        public void EnqueuePool(IRabbitProducer producer)
        {
            ProducersPool.Enqueue(producer);
        }
    }
}
