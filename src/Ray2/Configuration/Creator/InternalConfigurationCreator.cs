using Ray2.Configuration.Builder;
using Ray2.Configuration.Validator;
using Ray2.EventProcess;
using System;
using System.Linq;
using System.Reflection;

namespace Ray2.Configuration.Creator
{
    public class InternalConfigurationCreator : IInternalConfigurationCreator
    {
        private  InternalConfigurationBuilder builder;
        private readonly IInternalConfigurationValidator _validator;
        private readonly IEventProcessOptionsCreator _eventProcessOptionsCreator;
        private readonly IEventPublishOptionsCreator _eventPublishOptionsCreator;
        private readonly IEventSourceOptionsCreator _eventSourceOptionsCreator;
        public InternalConfigurationCreator(IServiceProvider services, IInternalConfigurationValidator validator, IEventProcessOptionsCreator eventProcessOptionsCreator, IEventPublishOptionsCreator eventPublishOptionsCreator, IEventSourceOptionsCreator eventSourceOptionsCreator)
        {
            this._validator = validator;
            this._eventProcessOptionsCreator = eventProcessOptionsCreator;
            this._eventPublishOptionsCreator = eventPublishOptionsCreator;
            this._eventSourceOptionsCreator = eventSourceOptionsCreator;
        }
        public InternalConfiguration Create()
        {
            builder = new InternalConfigurationBuilder();

            this.LoadConfiguration();

            var configuration = builder.Build();

            _validator.IsValid(configuration);

            return configuration;
        }
        public void LoadConfiguration()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic);
            foreach (var assembly in assemblies)
            {
                this.LoadEventSourcing(assembly);
                this.LoadEventProcessor(assembly);
                this.LoadEventInfo(assembly);
            }
        }
        private void LoadEventSourcing(Assembly assembly)
        {
            var estype = typeof(IRay);
            var allType = assembly.GetExportedTypes().Where(t => estype.IsAssignableFrom(t) 
                && t.IsAbstract == false 
                && t.IsClass == true);
            foreach (var type in allType)
            {
                var eventSourceOptions = this._eventSourceOptionsCreator.Create(type);
                this.builder.WithEventSourceOptions(eventSourceOptions);

                //Create an event publishing configuration
                var eventPublishOptions = this._eventPublishOptionsCreator.Create(type);
                if (eventPublishOptions != null)
                {
                    this.builder.WithEventPublishOptions(type.FullName, eventPublishOptions);
                }
            }
        }

        private void LoadEventProcessor(Assembly assembly)
        {
            var eptype = typeof(IEventProcessor);
            var allType = assembly.GetExportedTypes().Where(t => eptype.IsAssignableFrom(t) 
                && t.IsAbstract == false 
                && t.IsClass == true);
            foreach (var type in allType)
            {
                var eventProcessOptions = this._eventProcessOptionsCreator.Create(type);
                this.builder.WithEventProcessOptions(eventProcessOptions);
            }
        }

        private void LoadEventInfo(Assembly assembly)
        {
            var eventType = typeof(IEvent);
            var allType = assembly.GetExportedTypes().Where(t => eventType.IsAssignableFrom(t)
                && t.IsClass == true
                && t.IsAbstract == false
                && t.GetConstructors().Any(c => c.GetParameters().Length == 0));
            foreach (var type in allType)
            {
                if (Activator.CreateInstance(type) is IEvent msg)
                {
                    EventInfo info = new EventInfo();
                    info.Type = type;
                    if (!string.IsNullOrEmpty(msg.TypeCode))
                        info.Name=msg.TypeCode;
                    else
                        info.Name = type.FullName;
                    this.builder.WithEventInfo(info);

                    //Create an event publishing configuration
                    var eventPublishOptions = this._eventPublishOptionsCreator.Create(type);
                    if (eventPublishOptions != null)
                    {
                        this.builder.WithEventPublishOptions(type.FullName, eventPublishOptions);
                    }
                }
            }
        }
    }
}
