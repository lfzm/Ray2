using Ray2.MQ;

namespace Ray2.EventSource
{
    public class EventTransactionModel<TStateKey>
    {
        public EventTransactionModel(IEvent<TStateKey> @event, MQPublishType publishType)
        {
            this.Event = @event;
            this.PublishType = publishType;
        }
        /// <summary>
        /// Event
        /// </summary>
        public IEvent<TStateKey> Event { get;}
        /// <summary>
        /// MQ publish type
        /// </summary>
        public MQPublishType PublishType { get; }
    }
}
