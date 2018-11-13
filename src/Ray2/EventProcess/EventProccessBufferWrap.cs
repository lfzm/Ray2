using Ray2.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.EventProcess
{
    public class EventProccessBufferWrap : IDataflowBufferWrap
    {
        public EventProccessBufferWrap(IEvent @event)
        {
            this.Event = @event;
            this.TaskSource = new TaskCompletionSource<bool>();
        }
        public TaskCompletionSource<bool> TaskSource { get; }
        public IEvent Event { get; }
        public bool Result { get; set; }
    }
}
