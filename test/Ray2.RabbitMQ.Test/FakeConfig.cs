using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Runtime;
using Ray2.Configuration;
using Ray2.MQ;
using Ray2.RabbitMQ.Configuration;
using Ray2.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.RabbitMQ.Test
{
    public static class FakeConfig
    {
        public const string ProviderName = "Default";
        public static RabbitOptions Options = new RabbitOptions()
        {
            HostName = "192.168.1.250",
            UserName= "admin",
            Password = "admin"
        };
        public static IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions().Configure<RabbitOptions>(ProviderName, opt =>
            {
                opt.HostName = Options.HostName;
                opt.UserName = Options.Password;
                opt.ConnectionPoolCount = opt.ConnectionPoolCount;
                opt.ConsumeOptions = opt.ConsumeOptions;
                opt.HostNames = opt.HostNames;
                opt.SerializationType = opt.SerializationType;
                opt.VirtualHost = opt.VirtualHost;
            });
            services.AddLogging();
            Mock<IInternalConfiguration> internalConfiguration = new Mock<IInternalConfiguration>();
            //var type = typeof(TestEvent);
            //string name = type.FullName;
            //internalConfiguration.Setup(f => f.GetEvenType(name, out type)).Returns(true);
            services.AddSingleton<IInternalConfiguration>(internalConfiguration.Object);
            services.AddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>));
            services.AddSingletonNamedService<ISerializer, JsonSerializer>(SerializationType.JsonUTF8);

            services.AddSingletonNamedService<IEventPublisher>(ProviderName, (sp, n) =>
            {
                return new EventPublisher(sp, n);
            });
            services.AddSingletonNamedService<IEventSubscriber>(ProviderName, (sp, n) =>
            {
                return new EventSubscriber(sp, n);
            });
            services.AddSingletonNamedService<IRabbitChannelFactory>(ProviderName, (sp, n) =>
            {
                return new RabbitChannelFactory(sp, n);
            });
            return services.BuildServiceProvider();
        }
    }
}
