using System;
using System.Collections.Generic;

namespace Ray2.Configuration
{
    public class EventProcessOptions
    {
        public EventProcessOptions(string processorName, string processorFullName, ProcessorType processorType, string eventSourceName, int onceProcessCount, TimeSpan onceProcessTimeout, StatusOptions statusOptions, IList<EventSubscribeOptions> subscribeOptions)
        {
            this.ProcessorName = processorName;
            this.ProcessorFullName = processorFullName;
            this.EventSourceName = eventSourceName;
            this.OnceProcessCount = onceProcessCount;
            this.OnceProcessTimeout = onceProcessTimeout;
            this.StatusOptions = statusOptions;
            this.SubscribeOptions = subscribeOptions;
        }
        public string ProcessorName { get; private set; }
        public string EventSourceName { get; private set; }
        public string ProcessorFullName { get; private set; }
        public ProcessorType ProcessorType { get; private set; }
        public int OnceProcessCount { get; private set; }
        public TimeSpan OnceProcessTimeout { get; private set; }
        public StatusOptions StatusOptions { get; private set; }
        public IList<EventSubscribeOptions> SubscribeOptions { get; private set; }
    }

    /// <summary>
    /// Event processor type
    /// </summary>
    public enum ProcessorType
    {
        GrainProcessor,
        SimpleProcessor
    }
}
