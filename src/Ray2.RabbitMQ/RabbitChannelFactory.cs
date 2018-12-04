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

        public async Task<IRabbitChannel> GetChannel()
        {
            IRabbitConnection connection;
            if (Interlocked.Decrement(ref this.RemainderCount) > 0)
            {
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
                    await Task.Delay(1000);
                    return await this.GetChannel();
                }
            }
            //Continue to line up
            ConnectionPoll.Enqueue(connection);
            return connection.CreateChannel();
        }
    }
}
