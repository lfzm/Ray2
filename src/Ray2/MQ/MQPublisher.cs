using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Ray2.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ray2.EventSource;

namespace Ray2.MQ
{
    public class MQPublisher : IMQPublisher
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly EventPublishOptions _options;
        private readonly IEventPublisher _publisher;
        public MQPublisher(IServiceProvider serviceProvider, EventPublishOptions options)
        {
            this._options = options;
            this._serviceProvider = serviceProvider;
            this._logger = this._serviceProvider.GetRequiredService<ILogger<MQPublisher>>();
            this._publisher = this._serviceProvider.GetRequiredServiceByName<IEventPublisher>(options.MQProvider);
        }

        public Task<bool> Publish(IEvent @event)
        {
            try
            {
                var message = new EventModel(@event){};
                return this._publisher.Publish(_options.Topic, message);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, @event.ToString());
                return Task.FromResult(false);
            }
        }

        public Task<bool> Publish(IList<IEvent> events)
        {
            try
            {
                List<EventModel> messages = new List<EventModel>();
                foreach (var e in events)
                {
                    var message = new EventModel(e) { };
                    messages.Add(message);
                }
                return this._publisher.Publish(_options.Topic, messages);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "A lot of publish errors");
                return Task.FromResult(false);
            }
        }
    }
}
