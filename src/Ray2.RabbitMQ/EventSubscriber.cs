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
using System.Threading;
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
        private readonly Timer _monitorTimer;
        private readonly string providerName;
        private readonly Dictionary<string, IList<IRabbitConsumer>> ConsumerPool = new Dictionary<string, IList<IRabbitConsumer>>();
        public EventSubscriber(IServiceProvider serviceProvider, string providerName)
        {
            this.providerName = providerName;
            this._serviceProvider = serviceProvider;
            this._logger = this._serviceProvider.GetRequiredService<ILogger<EventSubscriber>>();
            this._options = serviceProvider.GetRequiredService<IOptionsSnapshot<RabbitOptions>>().Get(providerName);
            this._eventProcessorFactory = serviceProvider.GetRequiredService<IEventProcessorFactory>();
            this._serializer = this._serviceProvider.GetRequiredServiceByName<ISerializer>(_options.SerializationType);
            var _channelFactory = serviceProvider.GetRequiredServiceByName<IRabbitChannelFactory>(this.providerName);
            this._monitorTimer = new Timer(state => { MonitorConsumer().Wait(); }, null, TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10));
        }
        public async Task Subscribe(string group, string topic)
        {
            var options = this.GetConsumeOptions(group, topic);
            IEventProcessor processor = this._eventProcessorFactory.Create(group);
            IRabbitConsumer consumer = new RabbitConsumer(_serviceProvider, providerName, this._serializer);
            await consumer.Subscribe(group, topic, processor, options);
            //Collected to detect whether it is alive
            if (this.ConsumerPool.TryGetValue($"{group}@{topic}", out IList<IRabbitConsumer> list))
            {
                list.Add(consumer);
            }
            else
            {
                IList<IRabbitConsumer> consumers = new List<IRabbitConsumer>();
                consumers.Add(consumer);
                this.ConsumerPool.Add($"{group}@{topic}", consumers);
            }
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

        public async Task Stop()
        {
            foreach (var consumers in ConsumerPool.Values)
            {
                foreach (var consumer in consumers)
                {
                    await consumer.Close();
                }
            }
            ConsumerPool.Clear();
        }


        /// <summary>
        /// Monitor consumer availability and expansion
        /// </summary>
        /// <returns></returns>
        public async Task MonitorConsumer()
        {
            foreach (var key in ConsumerPool.Keys)
            {
                IList<IRabbitConsumer> consumers = ConsumerPool[key];
                //Check if you need to expand
                //Check that more than half of the consumers vote to expand and expand
                if ((consumers.Count / 2) <= consumers.Where(f => f.IsExpand()).Count())
                {
                    IRabbitConsumer consumer = consumers.First();
                    if (consumers.Count < consumer.Options.MaxConsumerCount)
                    {
                        await this.Subscribe(consumer.Queue, consumer.Exchange);
                    }
                }

                //Check if the consumer is restarted
                for (int i = 0; i < consumers.Count; i++)
                {
                    IRabbitConsumer consumer = consumers[i];
                    if (await this.RestartConsumer(consumer))
                    {
                        //Remove old consumers
                        consumers.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public async Task<bool> RestartConsumer(IRabbitConsumer consumer)
        {
            try
            {
                if (consumer.IsAvailable())
                {
                    await this.Subscribe(consumer.Queue, consumer.Exchange);
                    await consumer.Close();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Restart {consumer.Queue}&{consumer.Exchange}consumer failed");
                return false;
            }
        }
    }
}
