using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace Ray2.RabbitMQ
{
    public class RabbitChannel : IRabbitChannel
    {
        public IModel Model => throw new NotImplementedException();
    }
}
