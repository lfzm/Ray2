using Ray2.Storage;
using System.Threading.Tasks;

namespace Ray2.EventSource
{
    public class EventBufferWrap
    {
        public EventBufferWrap(EventSingleStorageModel @event)
        {
            this.Value = @event;
            this.TaskSource = new TaskCompletionSource<bool>();
        }
     
        public TaskCompletionSource<bool> TaskSource { get; }
        public EventSingleStorageModel Value { get; }
        public bool Result { get; set; }
    }

    
}
