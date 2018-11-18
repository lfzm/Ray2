using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray2.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    public class MQSubscriber : IMQSubscriber
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IInternalConfiguration _configuration;
        private readonly ILogger _logger;
        public MQSubscriber(IServiceProvider serviceProvider, IInternalConfiguration configuration, ILogger<MQSubscriber> logger)
        {
            this._serviceProvider = serviceProvider;
            this._configuration = configuration;
            this._logger = logger;
        }

        public async Task Start()
        {
            var options = this._configuration.GetEventProcessOptionsList();
            if (options == null || options.Count == 0)
                return;
            foreach (var o in options)
            {
                await this.StartSubscribe(o.ProcessorName, o.SubscribeOptions);
            }
        }

        /// <summary>
        /// Start subscribing to a single processor event
        /// </summary>
        /// <param name="processorName">This is the processor name</param>
        /// <param name="subscribeOptions">This is the subscribing config</param>
        /// <returns></returns>
        public async Task StartSubscribe(string processorName, IList<EventSubscribeOptions> subscribeOptions)
        {
            foreach (var sub in subscribeOptions)
            {
                var eventSubscriber = this._serviceProvider.GetRequiredServiceByName<IEventSubscriber>(sub.MQProvider);
                await eventSubscriber.Subscribe(processorName, sub.Topic);
            }
        }
    }
}
