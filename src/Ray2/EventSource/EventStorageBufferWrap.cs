using Ray2.Internal;
using Ray2.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace Ray2.EventSource
{
    public class EventStorageBufferWrap: IDataflowBufferWrap
    {
        public EventStorageBufferWrap(EventSingleStorageModel @event)
        {
            this.Value = @event;
            this.TaskSource = new TaskCompletionSource<bool>();
        }
        public TaskCompletionSource<bool> TaskSource { get; }
        public EventSingleStorageModel Value { get; }
        public bool Result { get; set; }
    }


}
