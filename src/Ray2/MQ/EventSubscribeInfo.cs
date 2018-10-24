using Ray2.Configuration;

namespace Ray2.MQ
{
    public class EventSubscribeInfo
    {
        public EventSubscribeInfo(SubscribeOptions config)
        {
            this.Topic = config.Topic;
            this.Group = config.Group;
            this.MQProvider = config.ProviderName;
        }
        public string MQProvider { get; set; }
        public string Topic { get; set; }
        public string Group { get; set; }

    }
}
