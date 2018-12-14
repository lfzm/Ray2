using Ray2.Internal;
using Ray2.MQ;
using System.Threading.Tasks;

namespace Ray2.EventSource
{
    public class EventTransactionBufferWrap<TStateKey> : IDataflowBufferWrap
    {
        public EventTransactionBufferWrap(IEvent<TStateKey> @event, MQPublishType type)
        {
            this.Value = @event;
            this.TaskSource = new TaskCompletionSource<bool>();
            this.Type = type;
        }
        public TaskCompletionSource<bool> TaskSource { get; }
        public IEvent<TStateKey> Value { get; }
        public MQPublishType  Type { get;  }
    }
}
