using Ray2.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.EventSource
{
    public class EventTransactionBufferWrap<TStateKey> : IDataflowBufferWrap
    {
        public EventTransactionBufferWrap(IEvent<TStateKey> @event, bool isPublish)
        {
            this.Value = @event;
            this.TaskSource = new TaskCompletionSource<bool>();
            this.IsPublish = isPublish;
        }
        public TaskCompletionSource<bool> TaskSource { get; }
        public IEvent<TStateKey> Value { get; }
        public bool IsPublish { get; set; }
    }
}
