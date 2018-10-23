using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Runtime;
using Ray2.Configuration;
using Ray2.EventProcess;
using Ray2.EventSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ray2
{
    public class RayBuilder : IRayBuilder
    {
        public IServiceCollection Services { get; }
        public IConfiguration Configuration { get; }

        private readonly List<SubscribeOptions> subscribeConfigs = new List<SubscribeOptions>();
        public RayBuilder(IServiceCollection services, IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.Services = services;
        }

        public void Build()
        {
            this.LoadEventTypes();
            this.LoadConfig();
        }

        public void LoadConfig()
        {
            var assemblyList = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic);
            foreach (var assembly in assemblyList)
            {
                this.LoadEventProcessConfig(assembly);
                this.LoadEventSourcingConfig(assembly);
            }
        }
        private void LoadEventSourcingConfig(Assembly assembly)
        {
            var estype = typeof(IRay);
            var allType = assembly.GetExportedTypes().Where(t => estype.IsAssignableFrom(t) && t.IsAbstract == false && t.IsClass == true);
            foreach (var type in allType)
            {
                var attr = type.GetCustomAttribute<EventSourcingAttribute>();
                if (attr == null)
                {
                    throw new RayConfigException($"The {type.FullName}  does not have an EventSourcingAttribute configured.");
                }
                this.LoadEventSourcingConfig(type, attr);
            }
        }
        private void LoadEventSourcingConfig(Type type, EventSourcingAttribute attr)
        {
            EventSourceOptions config = new EventSourceOptions(attr);
            config.Verify();  //verify event process config

            if (RayConfig.EventProcessors.ContainsKey(config.EventSourceName))
            {
                throw new RayConfigException($"Configuring Event Sources ' Name={config.EventSourceName} cannot be repeated");
            }
            RayConfig.EventSources.Add(config.EventSourceName, config);
        }

        private void LoadEventProcessConfig(Assembly assembly)
        {
            var eptype = typeof(IEventProcessor);
            var allType = assembly.GetExportedTypes().Where(t => eptype.IsAssignableFrom(t) && t.IsAbstract == false && t.IsClass == true);
            foreach (var type in allType)
            {
                var attrs = type.GetCustomAttributes<EventSubscribeAttribute>();
                if (attrs.Count() == 0)
                {
                    throw new RayConfigException($"The {type.FullName} processor does not have an EventSubscribeAttribute configured.");
                }
                foreach (var attr in attrs)
                {
                    this.LoadEventProcessConfig(type, attr);
                    this.LoadMQSubscribeConfig(attr);
                }
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
        private void LoadMQSubscribeConfig(EventSubscribeAttribute attr)
        {
            SubscribeOptions subConfig = new SubscribeOptions
            {
                Group = attr.Name,
                Topic = attr.EventSourceName,
                MQProvider = attr.MQProvider
            };
            RayConfig.MQSubscribes.Add(subConfig);
        }
        private void LoadEventTypes()
        {
            EventTypeCache etd = new EventTypeCache();
            etd.Initialize();
            this.Services.AddSingleton<IEventTypeCache>(etd);
        }

    }
}
