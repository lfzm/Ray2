using Ray2.MQ;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ
{
    public class EventSubscriber : IEventSubscriber
    {
        public Task Subscribe(string group, string topic)
        {
            throw new NotImplementedException();
        }
    }
}
