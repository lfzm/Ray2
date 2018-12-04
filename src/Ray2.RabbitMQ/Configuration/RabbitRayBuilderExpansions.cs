using Microsoft.Extensions.Configuration;
using Orleans.Runtime;
using Ray2;
using Ray2.MQ;
using Ray2.RabbitMQ;
using Ray2.RabbitMQ.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitRayBuilderExpansions
    {
        /// <summary>
        /// Add RabbitMQ 
        /// </summary>
        /// <param name="build"><see cref="IRayBuilder>"/></param>
        /// <param name="name">Provider name</param>
        /// <param name="configuration">RabbitMQ configuration</param>
        /// <returns></returns>
        public static IRayBuilder AddRabbitMQ(this IRayBuilder build, string name, IConfiguration configuration)
        {
            build.Services.Configure<RabbitOptions>(name, configuration);
            return build.AddRabbitMQ(name);
        }

        /// <summary>
        /// Add RabbitMQ 
        /// </summary>
        /// <param name="build"><see cref="IRayBuilder>"/></param>
        /// <param name="name">Provider name</param>
        /// <param name="configuration">RabbitMQ configuration</param>
        /// <returns></returns>
        public static IRayBuilder AddRabbitMQ(this IRayBuilder build, string name, Action<RabbitOptions> configuration)
        {
            build.Services.Configure<RabbitOptions>(name, configuration);
            return build.AddRabbitMQ(name);
        }

        /// <summary>
        ///  Add RabbitMQ 
        /// </summary>
        /// <param name="build"><see cref="IRayBuilder>"/></param>
        /// <param name="name">Provider name</param>
        /// <returns></returns>
        private static IRayBuilder AddRabbitMQ(this IRayBuilder build, string name)
        {
            build.Services.AddSingletonNamedService<IEventPublisher>(name, (sp, n) =>
            {
                return new EventPublisher(sp, n);
            });
            build.Services.AddSingletonNamedService<IEventSubscriber>(name, (sp, n) =>
            {
                return new EventSubscriber(sp, n);
            });
            build.Services.AddSingletonNamedService<IRabbitChannelFactory>(name, (sp, n) =>
            {
                return new RabbitChannelFactory(sp, n);
            });
            return build;
        }
    }
}
