using Ray2.EventSource;
using Ray2.MQ;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ
{
    public class EventPublisher : IEventPublisher
    {
        public Task<bool> Publish(string topic, EventModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Publish(string topic, IList<EventModel> model)
        {
            throw new NotImplementedException();
        }
    }
}
