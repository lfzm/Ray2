using Ray2.Configuration;

namespace Ray2.MQ
{
    public class EventSubscribeInfo
    {
        public EventSubscribeInfo(SubscribeOptions config)
        {
            this.Topic = config.Topic;
            this.Group = config.Group;
        }
        public string Topic { get; set; }
        public string Group { get; set; }

    }
}
