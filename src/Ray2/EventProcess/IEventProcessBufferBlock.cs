using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.EventProcess
{
    public interface IEventProcessBufferBlock
    {
        Task SendAsync(IEvent @event);
    }
}
