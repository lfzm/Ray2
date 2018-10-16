using System;

namespace Ray2.Configuration
{
    public class EventProcessingOptions
    {
        public string EventProcessorName { get; set; }
        public string EventSourcingName { get; set; }
        public int OnceProcessCount { get; set; }
        public TimeSpan OnceProcessTimeout { get; set; }
        public StatusOptions StatusOptions { get; set; }
        public SubscribeOptions SubscribeOptions { get; set; }
    }
}
