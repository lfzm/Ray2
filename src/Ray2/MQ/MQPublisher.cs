using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray2.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    public class MQPublisher : IMQPublisher
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private IEventPublisher publisher;
        private EventPublishOptions config;
        public MQPublisher(ILogger logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public void Injection(EventPublishOptions config)
        {
            this.config = config;
            this.publisher = this.serviceProvider.GetRequiredServiceByName<IEventPublisher>(config.MQProvider);
        }

        public Task<bool> Publish(IEvent @event)
        {
            try
            {
                if (this.config == null)
                    return Task.FromResult(false);
                var message = new EventPublishMessage()
                {
                    Event = @event,
                    TypeCode = @event.TypeCode
                };
                return this.publisher.Publish(config.Topic, message);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, @event.ToString());
                return Task.FromResult(false);
            }
        }

        public Task<bool> Publish(IList<IEvent> events)
        {
            try
            {
                if (this.config == null)
                    return Task.FromResult(false);
                List<EventPublishMessage> messages = new List<EventPublishMessage>();
                foreach (var e in events)
                {
                    var message = new EventPublishMessage()
                    {
                        Event = e,
                        TypeCode = e.TypeCode
                    };
                    messages.Add(message);
                }
                return this.publisher.Publish(config.Topic, messages);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "A lot of publish errors");
                return Task.FromResult(false);
            }
        }
    }
}
