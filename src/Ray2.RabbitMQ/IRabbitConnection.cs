using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.RabbitMQ
{
    public interface IRabbitConnection
    {
        IRabbitChannel CreateChannel();
        void Close();
    }
}
