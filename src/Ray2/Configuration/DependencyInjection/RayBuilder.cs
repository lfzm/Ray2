using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ray2.Configuration;
using Ray2.Configuration.Creator;
using Ray2.Configuration.Validator;
using Ray2.EventProcess;
using Ray2.EventSource;
using Ray2.MQ;
using Ray2.Serialization;
using Orleans.Runtime;
using Ray2.Internal;

namespace Ray2
{
    public class RayBuilder : IRayBuilder
    {
        public IServiceCollection Services { get; }
        public IConfiguration Configuration { get; }
        public RayBuilder(IServiceCollection services, IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.Services = services;
        }
        public void Build()
        {
            this.AddCoreServices();

            this.AddConfigurationServices();

            this.AddInternalConfiguration();
        }

        private void AddCoreServices()
        {
            this.Services.AddOptions();
            this.Services.AddTransient(typeof(IEventSourcing<,>), typeof(EventSourcing<,>));
            this.Services.AddTransient<IEventSourcing, EventSourcing>();
            this.Services.AddTransient(typeof(IEventProcessCore<,>), typeof(EventProcessCore<,>));
            this.Services.AddTransient<IEventProcessCore, EventProcessCore>();

            this.Services.AddSingleton<IEventProcessorFactory, EventProcessorFactory>();
            this.Services.AddSingleton<IMQSubscriber, MQSubscriber>();
            this.Services.AddSingleton<IMQPublisher, MQPublisher>();
            this.Services.AddSingleton<IDataflowBufferBlockFactory, DataflowBufferBlockFactory>();
            this.Services.AddSingletonNamedService<ISerializer, JsonSerializer>(SerializationType.JsonUTF8);
        }

        private void AddConfigurationServices()
        {
            this.Services.AddSingleton<IInternalConfigurationCreator, InternalConfigurationCreator>();
            this.Services.AddSingleton<IEventProcessOptionsCreator, EventProcessOptionsCreator>();
            this.Services.AddSingleton<IEventSourceOptionsCreator, EventSourceOptionsCreator>();
            this.Services.AddSingleton<IEventPublishOptionsCreator, EventPublishOptionsCreator>();
            this.Services.AddSingleton<IEventSubscribeOptionsCreator, EventSubscribeOptionsCreator>();

            this.Services.AddSingleton<IInternalConfigurationValidator, InternalConfigurationFluentVaildator>();
            this.Services.AddSingleton<EventProcessOptionsFluentVaildator>();
            this.Services.AddSingleton<EventPublishOptionsFluentVaildator>();
            this.Services.AddSingleton<EventSourceOptionsFluentVaildator>();
            this.Services.AddSingleton<EventSubscribeOptionsFluentVaildator>();
        }

        private void AddInternalConfiguration()
        {
            var configuration = new InternalConfigurationCreator().Create();
            this.Services.AddSingleton<IInternalConfiguration>(configuration);
            this.Services.BuildServiceProvider().GetRequiredService<IInternalConfigurationValidator>().IsValid(configuration);

            //Inject the Processor into the DI system
            var processList = configuration.EventProcessOptionsList;
            if (processList == null || processList.Count == 0)
                return;
            foreach (var p in processList)
            {
                if (p.ProcessorType == ProcessorType.SimpleProcessor)
                {
                    this.Services.AddSingleton(p.ProcessorHandle);
                }
            }
        }
    }
}
