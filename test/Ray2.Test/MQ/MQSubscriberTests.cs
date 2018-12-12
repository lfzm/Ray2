using Ray2.MQ;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Moq;
using Ray2.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;
using TestStack.BDDfy;
using System.Threading.Tasks;

namespace Ray2.Test.MQ
{
    public class MQSubscriberTests
    {
        private readonly MQSubscriber subscriber;
        private readonly Mock<IEventSubscriber> eventSubscriber = new Mock<IEventSubscriber>();
        private readonly Mock<IInternalConfiguration> internalConfiguration = new Mock<IInternalConfiguration>();
        private readonly string providerName = "Default";
        private readonly string name = "group";
        private int count = 0;
        public MQSubscriberTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>));
            services.AddSingletonNamedService(providerName, (s, key) => eventSubscriber.Object);
            services.AddSingletonNamedService(providerName+"1", (s, key) => eventSubscriber.Object);

            IServiceProvider sp = services.BuildServiceProvider();

            subscriber = new MQSubscriber(sp, internalConfiguration.Object, sp.GetRequiredService<ILogger<MQSubscriber>>());
        }

        [Fact]
        public void should_Start()
        {
            this.Given(f => f.GivenInitConfiguration())
                .Given(f => f.GivenInitEventSubscriber())
                .When(f => f.WhenStart())
                .Then(f => ThenExeNumber(2))
                .BDDfy();
        }

        [Fact]
        public void should_Stop()
        {
            this.Given(f => f.GivenInitConfiguration())
               .Given(f => f.GivenInitEventSubscriber())
               .When(f => f.WhenStop())
               .Then(f => ThenExeNumber(2))
               .BDDfy();
        }


        private void GivenInitConfiguration()
        {
            var opts = new List<EventProcessOptions>();
            EventProcessOptions processOptions = new EventProcessOptions(name,null, ProcessorType.GrainProcessor,null,null,0,TimeSpan.FromDays(1),null,new List<EventSubscribeOptions>());
            opts.Add(processOptions);
            processOptions.SubscribeOptions.Add(new EventSubscribeOptions(providerName, name, name));
            processOptions.SubscribeOptions.Add(new EventSubscribeOptions(providerName + "1", name, name));
            internalConfiguration.Setup(f => f.EventProcessOptionsList).Returns(opts);
        }

        private void GivenInitEventSubscriber()
        {
            eventSubscriber.Setup(f => f.Stop()).Returns(Task.CompletedTask).Callback(() => { this.count++; });
            eventSubscriber.Setup(f => f.Subscribe(name, name)).Returns(Task.CompletedTask).Callback(() => { this.count++; });
        }

        private void WhenStart()
        {
            this.subscriber.Start().Wait();
        }

        private void WhenStop()
        {
            this.subscriber.Stop();
        }

        private void ThenExeNumber(int number)
        {
            Assert.Equal(number, this.count);
        }
    }
}
