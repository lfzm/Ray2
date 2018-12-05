using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.RabbitMQ
{
    public interface IRabbitChannel
    {
        IRabbitConnection Connection { get; }
        bool IsOpen();
        void Close();
        IModel Model { get; }
        uint MessageCount(string quene);
    }
}
