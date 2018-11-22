using Ray2.EventSource;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    /// <summary>
    /// this is the event publish  interface
    /// </summary>
    public interface IEventPublisher
    {
        Task<bool> Publish(string topic, EventModel model);
        Task<bool> Publish(string topic, IList<EventModel> model);

    }
}
