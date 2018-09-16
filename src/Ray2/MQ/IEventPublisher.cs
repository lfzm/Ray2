using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    /// <summary>
    /// this is the event publish  interface
    /// </summary>
    public interface IEventPublisher
    {
        Task<bool> Publish(string topic, EventPublishMessage model);
        Task<bool> Publish(string topic, IList<EventPublishMessage> model);

    }
}
