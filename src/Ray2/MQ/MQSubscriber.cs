using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray2.Configuration;
using System;
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

        /// <summary>
        /// Start subscribing to a single processor event
        /// </summary>
        /// <returns></returns>
        public Task Start()
        {
            return this.Do((sub, processorName) =>
            {
                var eventSubscriber = this._serviceProvider.GetRequiredServiceByName<IEventSubscriber>(sub.MQProvider);
                return eventSubscriber.Subscribe(processorName, sub.Topic);
            });
        }

        public Task Stop()
        {
            return this.Do((sub, processorName) =>
            {
                var eventSubscriber = this._serviceProvider.GetRequiredServiceByName<IEventSubscriber>(sub.MQProvider);
                return eventSubscriber.Stop();
            });
        }

        public async Task Do(Func<EventSubscribeOptions, string, Task> func)
        {
            var options = this._configuration.GetEventProcessOptionsList();
            if (options == null || options.Count == 0)
                return;
            foreach (var o in options)
            {
                foreach (var sub in o.SubscribeOptions)
                {
                    await func(sub, o.ProcessorName);
                }
            }
        }

    }
}
