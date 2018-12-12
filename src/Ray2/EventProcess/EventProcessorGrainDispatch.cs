using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Ray2.EventSource;
using System;
using System.Threading.Tasks;

namespace Ray2.EventProcess
{
    public class EventProcessorGrainDispatch : IEventProcessor
    {
        private readonly string _grainClassName;
        private readonly IServiceProvider _serviceProvider;
        private readonly IClusterClient client;
        public EventProcessorGrainDispatch(string grainClassName, IServiceProvider serviceProvider)
        {
            this._grainClassName = grainClassName;
            this._serviceProvider = serviceProvider;
            this.client = serviceProvider.GetRequiredService<IClusterClient>();
        }
        public Task<bool> Tell(EventModel model)
        {
            object id = model.Event.GetStateId();
            IEventProcessor eventProcessor;
            if (id is Guid _guid)
            {
                eventProcessor = client.GetGrain<IEventProcessor>(primaryKey: _guid, grainClassNamePrefix: _grainClassName);
            }
            else if (id is long _id)
            {
                eventProcessor = client.GetGrain<IEventProcessor>(primaryKey: _id, grainClassNamePrefix: _grainClassName);
            }
            else
            {
                eventProcessor = client.GetGrain<IEventProcessor>(primaryKey: id.ToString(), grainClassNamePrefix: _grainClassName);
            }
            return eventProcessor.Tell(model);
        }
    }
}
