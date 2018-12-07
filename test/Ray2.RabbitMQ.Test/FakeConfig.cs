using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Runtime;
using Ray2.Configuration;
using Ray2.EventProcess;
using Ray2.MQ;
using Ray2.RabbitMQ.Configuration;
using Ray2.RabbitMQ.Test.Model;
using Ray2.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.RabbitMQ.Test
{
    public static class FakeConfig
    {
        public static Mock<IEventProcessorFactory> EventProcessorFactory = new Mock<IEventProcessorFactory>();
        public static Mock<IInternalConfiguration> InternalConfiguration = new Mock<IInternalConfiguration>();

        public const string ProviderName = "Default";
        public static RabbitOptions Options = new RabbitOptions()
        {
            HostName = "192.168.1.250",
            UserName= "admin",
            Password = "admin",
            ConsumeOptions = new List<RabbitConsumeOptions>
            {
                new RabbitConsumeOptions()
                {
                    Group="subGroup",
                    Topic="subTopic",
                    NoticeRetriesCount=3
                }
            }
        };
        public static IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions().Configure<RabbitOptions>(ProviderName, opt =>
            {
                opt.HostName = Options.HostName;
                opt.UserName = Options.UserName;
                opt.Password = Options.Password;
                opt.ConnectionPoolCount = Options.ConnectionPoolCount;
                opt.ConsumeOptions = Options.ConsumeOptions;
                opt.HostNames = Options.HostNames;
                opt.SerializationType = Options.SerializationType;
                opt.VirtualHost = Options.VirtualHost;
                opt.ConsumeOptions = Options.ConsumeOptions;
            });
            services.AddLogging();
            var type = typeof(TestEvent);
            InternalConfiguration.Setup(f => f.GetEvenType(type.FullName, out type)).Returns(true);

            services.AddSingleton<IInternalConfiguration>(InternalConfiguration.Object);
            services.AddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>));
            services.AddSingletonNamedService<ISerializer, JsonSerializer>(SerializationType.JsonUTF8);
            services.AddSingleton<IEventProcessorFactory>(EventProcessorFactory.Object);
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
