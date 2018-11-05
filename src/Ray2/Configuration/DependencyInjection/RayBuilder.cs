using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ray2.Configuration;
using Ray2.Configuration.Creator;
using Ray2.Configuration.Validator;
using Ray2.EventProcess;
using Ray2.EventSource;
using Ray2.MQ;
using Ray2.Serialization;

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
            this.Services.AddTransient(typeof(IEventSourcing<,>), typeof(EventSourcing<,>));
            this.Services.AddTransient<IEventSourcing,EventSourcing>();
            this.Services.AddTransient(typeof(IEventProcessCore<,>), typeof(EventProcessCore<,>));
            this.Services.AddTransient<IEventProcessCore, EventProcessCore>();
            this.Services.AddTransient<IMQPublisher, MQPublisher>();

            this.Services.AddTransient<IStringSerializer, JsonSerializer>();
            this.Services.AddTransient<IByteSerializer, ProtobufSerializer>();
            this.Services.AddTransient<ISerializer, Serializer>();
        }

        private void AddConfigurationServices()
        {
            this.Services.AddSingleton<IInternalConfigurationCreator, InternalConfigurationCreator>();
            this.Services.AddSingleton<IEventProcessOptionsCreator, EventProcessOptionsCreator>();
            this.Services.AddSingleton<IEventSourceOptionsCreator, EventSourceOptionsCreator>();
            this.Services.AddSingleton<IEventPublishOptionsCreator, EventPublishOptionsCreator>();
            this.Services.AddSingleton<IEventSubscribeOptionsCreator, EventSubscribeOptionsCreator>();

            this.Services.AddSingleton<IInternalConfigurationValidator,InternalConfigurationFluentVaildator>();
            this.Services.AddSingleton<EventProcessOptionsFluentVaildator>();
            this.Services.AddSingleton<EventPublishOptionsFluentVaildator>();
            this.Services.AddSingleton<EventSourceOptionsFluentVaildator>();
            this.Services.AddSingleton<EventSubscribeOptionsFluentVaildator>();
            
        }

        private void AddInternalConfiguration()
        {
            var configurationCreator = this.Services.BuildServiceProvider().GetRequiredService<IInternalConfigurationCreator>();
            var configuration = configurationCreator.Create();
            this.Services.AddSingleton<IInternalConfiguration>(configuration);
        }
    }
}
