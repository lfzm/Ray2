using Ray2.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    /// <summary>
    /// this is the event publish  interface
    /// </summary>
    public interface IMQPublisher
    {
        void Injection( EventPublishOptions config);
        Task<bool> Publish(IEvent @event);
        Task<bool> Publish(IList<IEvent> events);
    }
}
