using Ray2.MQ;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Runtime;
using Ray2.Test.Model;
using Microsoft.Extensions.Logging;
using Xunit;
using Ray2.EventSource;
using System.Threading.Tasks;
using TestStack.BDDfy;

namespace Ray2.Test.MQ
{
    public class MQPublisherTests
    {
        private readonly MQPublisher publisher;
        private readonly Mock<IEventPublisher> eventPublisher = new Mock<IEventPublisher>();
        private readonly string providerName = "Default";
        private readonly string topic = "test";
        private bool Result;
        private Exception exception;
        private EventModel model;

        public MQPublisherTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>));
            services.AddSingletonNamedService(providerName, (s, key) => eventPublisher.Object);
            IServiceProvider sp = services.BuildServiceProvider();
            this.publisher = new MQPublisher(sp, sp.GetRequiredService<ILogger<MQPublisher>>());
        }
        [Fact]
        public void should_Publish_NotAttribute()
        {
            var e1 = TestEvent1.Create(1);
            this.Given(f => f.GivenInitEventPublisher())
                .When(f => f.WhenPublish(e1))
                .Then(f => f.ThenFailes())
                .BDDfy();
        }

        [Fact]
        public void should_Publish_Attribute()
        {
            var e = TestEvent.Create(1);
            this.Given(f => f.GivenInitEventPublisher())
                .When(f => f.WhenPublish(e))
                .Then(f => f.ThenSuccess())
                .BDDfy();
        }

        [Fact]
        public void should_GetAttributeOptions()
        {
            var e = TestEvent.Create(1);
            var options = this.publisher.GetAttributeOptions(e.GetType());
            Assert.NotNull(options);
            Assert.Equal(options.MQProvider, this.providerName);
            Assert.Equal(topic, options.Topic);

            var e1 = TestEvent1.Create(1);
            options = this.publisher.GetAttributeOptions(e1.GetType());
            Assert.Null(options);
        }

        private void GivenInitEventPublisher()
        {
            eventPublisher.Setup(f => f.Publish(topic, It.IsNotNull<EventModel>())).Returns(Task.FromResult(true)).Callback((string topic,EventModel m) =>model=m) ;
            eventPublisher.Setup(f => f.Publish(topic, It.IsNotNull<List<EventModel>>())).Returns(Task.FromResult(true)).Callback((string topic,EventModel m) => model = m);
        }

        private void WhenPublish(IEvent @event, string topic, string mqProviderName)
        {
            Result = this.publisher.Publish(@event, topic, mqProviderName).GetAwaiter().GetResult();
        }

        private void WhenPublish(IEvent @event)
        {
            try
            {
                Result = this.publisher.Publish(@event).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }
        private void WhenPublish(IList<IEvent> events)
        {
            this.publisher.Publish(events).GetAwaiter().GetResult();
        }

        private void WhenPublish(IList<IEvent> events, string topic, string mqProviderName)
        {
            this.publisher.Publish(events, topic, mqProviderName).GetAwaiter().GetResult();
        }

        private void ThenFailes()
        {
            Assert.False(this.Result);
            Assert.NotNull(this.exception);
        }
        private void ThenSuccess()
        {
            Assert.True(this.Result);
            Assert.Null(this.exception);
            Assert.NotNull(this.model);
        }
    }
}
