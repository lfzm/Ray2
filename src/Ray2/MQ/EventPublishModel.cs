namespace Ray2.MQ
{
    public class EventPublishModel
    {
        public EventPublishModel(IEvent evt, string topic,string mqProviderName, MQPublishType publishType = MQPublishType.Asynchronous)
        {
            this.Event = evt;
            this.Topic = topic;
            this.MQProviderName = mqProviderName;
            this.Type = publishType;
        }
        public IEvent Event { get; }
        /// <summary>
        /// MQ publish type
        /// </summary>
        public MQPublishType Type { get; }
        /// <summary>
        /// MQ Topic
        /// </summary>
        public string Topic { get; }
        /// <summary>
        /// MQ provider name
        /// </summary>
        public string MQProviderName { get; }
    }
}
