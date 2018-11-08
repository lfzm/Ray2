using System.Collections.Generic;

namespace Ray2.Storage
{
    public class EventStorageInfo
    {
        public string EventSource { get; set; }
        public string Provider { get; set; }
        public IEventStorage Storage { get; set; } 
        public IList<string> Tables { get; set; } = new List<string>();
    }
}
