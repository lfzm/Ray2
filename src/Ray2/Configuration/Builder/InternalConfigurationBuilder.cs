using System.Collections.Generic;

namespace Ray2.Configuration.Builder
{
    /// <summary>
    /// System internal configuration builder
    /// </summary>
    public class InternalConfigurationBuilder
    {
        InternalConfiguration configuration;
        public InternalConfigurationBuilder()
        {
            configuration = new InternalConfiguration();
        }

        public void WithEventProcessOptions(EventProcessOptions options)
        {
            this.configuration._eventProcessOptions.Add(options.ProcessorName, options);
            this.configuration._eventProcessOptionsByProcessor.Add(options.ProcessorFullName, options);
        }

        public void WithEventProcessOptions(IList<EventProcessOptions> options)
        {
            foreach (var item in options)
            {
                this.WithEventProcessOptions(item);
            }
        }
        public void WithEventSourceOptions(EventSourceOptions options)
        {
            this.configuration._eventSourceOptions.Add(options.EventSourceName, options);
            this.configuration._eventSourceOptionsBySource.Add(options.SourcingFullName, options);
        }
        public void WithEventPublishOptions(string name, EventPublishOptions options)
        {
            this.configuration._eventPublishOptions.Add(name, options);
        }
        public void WithEventInfo(EventInfo info)
        {
            this.configuration._eventInfos.Add(info.Name, info);
        }
        public InternalConfiguration Build()
        {
            return configuration;
        }
    }
}
