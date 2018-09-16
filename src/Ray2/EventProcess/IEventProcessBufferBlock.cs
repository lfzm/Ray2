using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.EventHandler
{
    public interface IEventProcessBufferBlock
    {
        Task SendAsync(IEvent @event);
    }
}
