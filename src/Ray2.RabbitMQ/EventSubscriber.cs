using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Ray2.EventProcess;
using Ray2.MQ;
using Ray2.RabbitMQ.Configuration;
using Ray2.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ
{
    public class EventSubscriber : IEventSubscriber
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventProcessorFactory _eventProcessorFactory;
        private readonly ISerializer _serializer;
        private readonly RabbitOptions _options;
        private readonly ILogger _logger;
        private readonly string providerName;
        private readonly Dictionary<string, IRabbitConsumer> consumerList = new Dictionary<string, IRabbitConsumer>();
        public EventSubscriber(IServiceProvider serviceProvider, string providerName)
        {
            this._serviceProvider = serviceProvider;
            this._logger = this._serviceProvider.GetRequiredService<ILogger<EventSubscriber>>();
            this._options = serviceProvider.GetRequiredService<IOptionsSnapshot<RabbitOptions>>().Get(providerName);
            this._eventProcessorFactory = serviceProvider.GetRequiredService<IEventProcessorFactory>();
            this._serializer = this._serviceProvider.GetRequiredServiceByName<ISerializer>(_options.SerializationType);

            this.providerName = providerName;

        }
        public async Task Subscribe(string group, string topic)
        {
            var options = this.GetConsumeOptions(group, topic);
            IEventProcessor processor = this._eventProcessorFactory.Create(group);
            IRabbitConsumer consumer = new RabbitConsumer(providerName, _serviceProvider, this._serializer);
            await consumer.Subscribe(group, topic, processor, options);
            //Collected to detect whether it is alive
            this.consumerList.Add($"{group}@{topic}", consumer);
        }

        /// <summary>
        /// Get consumer configuration
        /// </summary>
        /// <param name="group">group</param>
        /// <param name="topic">topic</param>
        /// <returns></returns>
        public RabbitConsumeOptions GetConsumeOptions(string group, string topic)
        {
            var options = this._options.ConsumeOptions.Where(f => f.Group == group && f.Topic == topic).FirstOrDefault();
            if (options != null)
                return options;
            else
                return new RabbitConsumeOptions();

        }
    }
}
