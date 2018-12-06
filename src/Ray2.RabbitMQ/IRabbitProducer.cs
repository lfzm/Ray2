using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ
{
    /// <summary>
    /// This is the producer interface of Rebbit.
    /// </summary>
    public interface IRabbitProducer
    {
        /// <summary>
        /// Is this producer available?
        /// </summary>
        /// <returns></returns>
        bool IsAvailable();

        void Close();
        Task<bool> Publish(string exchange, string routingKey, PublishMessage message);
    }
}
