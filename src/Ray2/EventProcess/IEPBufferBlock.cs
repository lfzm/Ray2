using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.EventProcess
{
    public interface IEPBufferBlock
    {
        Task SendAsync(IEvent @event);
    }
}
