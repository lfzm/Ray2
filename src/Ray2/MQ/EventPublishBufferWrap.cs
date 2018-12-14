using Ray2.Internal;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    public class EventPublishBufferWrap : IDataflowBufferWrap
    {
        public EventPublishBufferWrap(IEvent evt,string topic,string mqProviderName, MQPublishType publishType)
        {
            this.Value = evt;
            this.Topic = topic;
            this.Type = publishType;
            this.MQProviderName = mqProviderName;
            this.TaskSource = new TaskCompletionSource<bool>();
        }
        public TaskCompletionSource<bool> TaskSource { get; }
        /// <summary>
        /// publish event
        /// </summary>
        public IEvent Value { get; }
        /// <summary>
        /// MQ publish type
        /// </summary>
        public MQPublishType Type { get;  }
        /// <summary>
        /// MQ Topic
        /// </summary>
        public string Topic { get; }
        /// <summary>
        /// MQ provider name
        /// </summary>
        public string MQProviderName { get; }

        public Task<bool> Result;
    }
}
