using Ray2.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    public static class MQPublisherExtensions
    {
        private static  ConcurrentDictionary<Type, EventPublishOptions> eventPublishOptions = new ConcurrentDictionary<Type, EventPublishOptions>();

        public static  Task<bool> PublishAsync(this IMQPublisher publisher, IEvent @event, MQPublishType publishType = MQPublishType.Asynchronous)
        {
            var options = GetAttributeOptions(@event.GetType());
            if (options == null)
            {
                throw new Exception($"No eventPublishAttribute is marked for {@event.GetType().FullName} events");
            }
            return  publisher.PublishAsync(@event, options.Topic, options.MQProvider, publishType);
        }

        public static async Task PublishAsync(this IMQPublisher publisher, IList<IEvent> events, MQPublishType publishType = MQPublishType.Asynchronous)
        {
            if (events == null)
            {
                return;
            }
            foreach (var e in events)
            {
                await publisher.PublishAsync(e, publishType);
            }
        }

        public static async Task PublishAsync(this IMQPublisher publisher, IList<IEvent> events, string topic, string mqProviderName, MQPublishType publishType = MQPublishType.Asynchronous)
        {
            if (events == null || events.Count==0)
            {
                return;
            }
            foreach (var e in events)
            {
                await publisher.PublishAsync(e, topic, mqProviderName, publishType);
            }
        }

        public static EventPublishOptions GetAttributeOptions(Type type)
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
