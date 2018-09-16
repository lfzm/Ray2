using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.EventSources
{
    public interface IEventTypeCache
    {
        bool GetEventType(string name, out Type value);
    }
}
