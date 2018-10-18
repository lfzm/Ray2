using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray2.EventSource;
using Ray2.MQ;
using System;
using System.Threading.Tasks;

namespace Ray2.EventProcess
{
    public class EventProcessorDispatch : IEventProcessorDispatch
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        public EventProcessorDispatch(ILogger<EventProcessorDispatch> logger,IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }
        public async Task<bool> Notice(EventSubscribeInfo info, EventPublishMessage message)
        {
            try
            {
                //get processor
                var eventProcessor = this.serviceProvider.GetRequiredServiceByName<IEventProcessor>(info.Group);
                if (message.Event is IEvent @event)
                {
                    await eventProcessor.Tell(@event);
                    return true;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{info.Group}.{info.Topic}->{ message.TypeCode } process failed");
            }
            return false;
        }
    }
}
