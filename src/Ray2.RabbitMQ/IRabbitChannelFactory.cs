using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ
{
    public interface IRabbitChannelFactory
    {
        IRabbitChannel GetChannel();
    }
}
