using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray2.Configuration;
using Ray2.EventSource;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    public class MQPublisher : IMQPublisher
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private ConcurrentDictionary<Type, EventPublishOptions> eventPublishOptions = new ConcurrentDictionary<Type, EventPublishOptions>();

        public MQPublisher(IServiceProvider serviceProvider, ILogger<MQPublisher> logger)
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;
        }
        public async Task<bool> Publish(IEvent @event)
        {
            var options = this.GetAttributeOptions(@event.GetType());
            if (options == null)
            {
                throw new Exception($"No eventPublishAttribute is marked for {@event.GetType().FullName} events");
            }
            return await this.Publish(@event, options.Topic, options.MQProvider);
        }

        public async Task Publish(IList<IEvent> events)
        {
            if (events == null)
            {
                return;
            }
            foreach (var e in events)
            {
                await this.Publish(e);
            }
        }

        public Task<bool> Publish(IEvent @event, string topic, string mqProviderName)
        {
            var provider = this._serviceProvider.GetRequiredServiceByName<IEventPublisher>(mqProviderName);
            var model = new EventModel(@event);
            return provider.Publish(topic, model);
        }
        public Task Publish(IList<IEvent> events, string topic, string mqProviderName)
        {
            var provider = this._serviceProvider.GetRequiredServiceByName<IEventPublisher>(mqProviderName);
            List<EventModel> messages = new List<EventModel>();
            foreach (var e in events)
            {
                var message = new EventModel(e) { };
                messages.Add(message);
            }
            return provider.Publish(topic, messages);
        }

        public EventPublishOptions GetAttributeOptions(Type type)
        {
            return eventPublishOptions.GetOrAdd(type, (key) =>
             {
                 var attribute = type.GetCustomAttribute<EventPublishAttribute>();
                 if (attribute != null)
                 {
                     string fullName = type.FullName;
                     if (string.IsNullOrEmpty(attribute.Topic))
                     {
                         throw new ArgumentNullException($"EventPublishAttribute.Topic in {fullName} cannot be empty");
                     }
                     if (string.IsNullOrEmpty(attribute.MQProvider))
                     {
                         throw new ArgumentNullException($"EventPublishAttribute.MQProvider in {fullName} cannot be empty");
                     }
                     return new EventPublishOptions(attribute.Topic, attribute.MQProvider, fullName);
                 }
                 else
                 {
                     return null;
                 }
             });
        }


    }
}
