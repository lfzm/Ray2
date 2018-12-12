using Ray2.Configuration;
using System;

namespace Ray2
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EventProcessorAttribute : Attribute
    {
        public EventProcessorAttribute(string eventSourceName, string mqProviderName, string storageProviderName)
        {
            this.EventSourceName = eventSourceName;
            this.Topic = this.EventSourceName;

            this.MQProvider = mqProviderName;
            this.StorageProvider = storageProviderName;
        }
        public EventProcessorAttribute( Type eventSource, string mqProviderName, string storageProviderName):this(eventSource.Name,mqProviderName,storageProviderName)
        {
         
        }
        public string Name { get; set; }
        public string EventSourceName { get; set; }
        public string MQProvider { get; set; }
        public string Topic { get; set; }
        public string StorageProvider { get; set; }
        public string ShardingStrategy { get; set; }
        public int OnceProcessCount { get; set; } = 1;
        public TimeSpan OnceProcessTimeout { get; set; } = TimeSpan.FromMinutes(5);
        public StatusMode StatusMode { get; set; } = StatusMode.Asynchronous;
    }
}
