using Orleans;
using Orleans.Runtime;
using Ray2.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ray2.MQ
{
    public class MQManager : IMQManager, IStartupTask
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IGrainFactory grainFactory;
        public MQManager(IServiceProvider serviceProvider, IGrainFactory grainFactory)
        {
            this.serviceProvider = serviceProvider;
            this.grainFactory = grainFactory;
        }

        public Task Execute(CancellationToken cancellationToken)
        {
            return this.Start();
        }

        public async Task Start()
        {
            foreach (var config in RayConfig.MQSubscribes)
            {
                var provider = this.serviceProvider.GetServiceByName<IEventSubscriber>(config.MQProvider);
                await provider.Subscribe(new EventSubscribeInfo(config));
            }
        }
    }
}
