using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Ray2.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                var message = new EventPublishMessage()
                {
                    Event = @event,
                    TypeCode = @event.TypeCode
                };
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
