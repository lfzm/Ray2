using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ray2.RabbitMQ.Configuration;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ
{
    public class RabbitChannelFactory : IRabbitChannelFactory
    {
        private int RemainderCount;//Number of remaining connection pools
        private readonly ConcurrentQueue<IRabbitConnection> ConnectionPoll = new ConcurrentQueue<IRabbitConnection>();
        private readonly RabbitOptions Options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        public RabbitChannelFactory(IServiceProvider serviceProvider, string providerName)
        {
            this._serviceProvider = serviceProvider;
            this._logger = serviceProvider.GetRequiredService<ILogger<RabbitChannelFactory>>();
            this.Options = serviceProvider.GetRequiredService<IOptionsSnapshot<RabbitOptions>>().Get(providerName);
            this.RemainderCount = this.Options.ConnectionPoolCount > 0 ? this.Options.ConnectionPoolCount : 10;
        }

        public IRabbitChannel GetChannel()
        {
            IRabbitConnection connection;
            if (this.RemainderCount > 0)
            {
                Interlocked.Decrement(ref this.RemainderCount);
                connection = new RabbitConnection(this._serviceProvider, this.Options);
            }
            else
            {
                if (ConnectionPoll.TryDequeue(out IRabbitConnection conn))
                {
                    connection = conn;
                }
                else
                {
                    Task.Delay(1000).GetAwaiter().GetResult();
                    return this.GetChannel();
                }
            }
            if (connection.IsOpen())
            {
                //Continue to line up
                var channel = connection.CreateChannel();
                ConnectionPoll.Enqueue(connection);
                return channel;
            }
            else
            {
                Interlocked.CompareExchange(ref this.RemainderCount, 1, 0);
                return this.GetChannel();
            }
        }
    }
}
