using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.PostgreSQL.Serialization
{
    public interface IEventSerializer: ISerializer,IDisposable
    {
        object Deserialize(string eventTypeCode, byte[] bytes);
        object Deserialize(string eventTypeCode,string json);
        (byte[] bytes, string json) Serialize(IEvent @event);
    }
}
