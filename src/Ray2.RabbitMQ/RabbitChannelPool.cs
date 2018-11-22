using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ
{
    public class RabbitChannelPool : IRabbitChannelPool
    {
        public Task<IRabbitChannel> GetChannel()
        {
            throw new NotImplementedException();
        }
    }
}
