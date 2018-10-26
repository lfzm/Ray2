using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration.Creator
{
    public class EventProcessOptionsCreator : IEventProcessOptionsCreator
    {
        private readonly IEventSubscribeOptionsCreator _eventSubscribeOptionsCreator;
        public EventProcessOptionsCreator(IEventSubscribeOptionsCreator eventSubscribeOptionsCreator)
        {
            this._eventSubscribeOptionsCreator = eventSubscribeOptionsCreator;
        }

        public List<EventProcessOptions> Create(Type type)
        {
            var attrs = type.GetCustomAttributes<EventSubscribeAttribute>();
            if (attrs.Count() == 0)
            {
                throw new Exception($"The {type.FullName} processor does not have an EventSubscribeAttribute configured.");
            }
            foreach (var attr in attrs)
            {
                this.LoadEventProcessConfig(type, attr);
            }
        }

        private void LoadEventProcessConfig(Type type, EventSubscribeAttribute attr)
        {
            EventProcessOptions config = new EventProcessOptions(attr, type);
            config.Verify();  //verify event process config

            if (RayConfig.EventProcessors.ContainsKey(config.ProcessorName))
            {
                throw new RayConfigException($"Configuring Event Processors' Name={config.ProcessorName} cannot be repeated");
            }
            RayConfig.EventProcessors.Add(config.ProcessorName, config);

            //Inject the processor into the DI
            if (type.BaseType == typeof(Grain))
            {
                this.Services.AddSingletonNamedService<IEventProcessor>(config.ProcessorName, (IServiceProvider sp, string key) =>
                {
                    return new EventProcessorGrainDispatch(type.FullName, sp);
                });
            }
            else
            {
                this.Services.AddSingleton(type);
                this.Services.AddSingletonNamedService<IEventProcessor>(config.ProcessorName, (IServiceProvider sp, string key) =>
                {
                    return (IEventProcessor)sp.GetRequiredService(type);
                });
            }
        }
    }
}
