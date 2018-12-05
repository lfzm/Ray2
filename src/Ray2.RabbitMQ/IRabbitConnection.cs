using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.RabbitMQ
{
    public interface IRabbitConnection
    {
        IConnection Connection { get; }
        IRabbitChannel CreateChannel();
        void Close();
    }
}
