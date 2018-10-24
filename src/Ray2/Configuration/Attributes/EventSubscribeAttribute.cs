using Ray2.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EventSubscribeAttribute : Attribute
    {
        public string Name { get; set; }
        public string EventSourceName { get; set; }
        public string StorageProvider { get; set; }
        public string ShardingStrategy { get; set; }
        public int OnceProcessCount { get; set; } = 1;
        public TimeSpan OnceProcessTimeout { get; set; } = TimeSpan.FromMinutes(5);
        public StatusMode StatusMode { get; set; } = StatusMode.Asynchronous;
    }
}
