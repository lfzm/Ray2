using Moq;
using Orleans.Runtime;
using RabbitMQ.Client;
using Ray2.EventProcess;
using Ray2.EventSource;
using Ray2.RabbitMQ.Test.Model;
using Ray2.Serialization;
using System;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace Ray2.RabbitMQ.Test
{
    public class EventSubscriberTests
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ISerializer serializer;
        private readonly EventSubscriber subscriber;
        private string group = "EventSubscriberTests";
        private string topic = "EventSubscriberTests";
        private string exchange = "EventSubscriberTests";
        private EventModel model;
        public EventSubscriberTests()
        {
            this.serviceProvider = FakeConfig.BuildServiceProvider();
            this.subscriber = new EventSubscriber(this.serviceProvider, FakeConfig.ProviderName);
            this.serializer = serviceProvider.GetRequiredServiceByName<ISerializer>(FakeConfig.Options.SerializationType);
        }

        [Fact]
        public void should_GetConsumeOptions()
        {
            var options = this.subscriber.GetConsumeOptions(this.group, this.topic);
            Assert.NotNull(options);
        }
        [Fact]
        public void should_Subscribe_Normal()
        {
            EventModel model = new EventModel(TestEvent.Create(1));
            PublishMessage msg = new PublishMessage(model, this.serializer);
            this.topic += "Subscribe";
            this.exchange += "Subscribe";
            this.group += "Subscribe";
            this.Given(f => f.GivenInitProcessor())
                .When(f => f.WhenSubcribe())
                .When(f => f.WhenPublish(msg, 1))
                .Then(f => f.ThenProcess(model))
                .BDDfy();
        }
        [Fact]
        public void should_stop()
        {
            this.When(f => f.WhenSubcribeList(100))
                .When(f => f.WhenStop())
                .BDDfy();
        }

        [Fact]
        public void should_Expand_Normal()
        {
            this.topic += "Expand";
            this.exchange += "Expand";
            EventModel model = new EventModel(TestEvent.Create(1));
            PublishMessage msg = new PublishMessage(model, this.serializer);
            this.Given(f => f.GivenInitProcessor())
                .When(f => f.WhenSubcribe())
                .When(f => f.WhenPublish(msg, 5000))
                .When(f=>f.WhenMonitorConsumer())
                .BDDfy();
        }
        private void GivenInitProcessor()
        {
            Mock<IEventProcessor> processor = new Mock<IEventProcessor>();
            FakeConfig.EventProcessorFactory.Setup(f => f.Create(this.group)).Returns(processor.Object);
            processor.Setup(f => f.Tell(It.IsNotNull<EventModel>()))
            .Returns(Task.FromResult(true))
            .Callback<EventModel>(f =>
            {
                Task.Delay(50).Wait();
                this.model = f;
            });
        }
        private void WhenSubcribeList(int subCount)
        {
            for (int i = 0; i < subCount; i++)
            {
                this.subscriber.Subscribe(this.group + i, this.topic).GetAwaiter().GetResult();
            }
        }
        private void WhenSubcribe()
        {
            this.subscriber.Subscribe(this.group, this.topic).GetAwaiter().GetResult();
        }
        private void WhenMonitorConsumer()
        {
            for (int i = 0; i < 11; i++)
            {
                this.subscriber.MonitorConsumer().GetAwaiter().GetResult();
            }

        }
        private void WhenStop()
        {
            this.subscriber.Stop().GetAwaiter().GetResult();
        }
        private void WhenPublish(PublishMessage msg, int count = 1)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                UserName = FakeConfig.Options.UserName,//UserName
                Password = FakeConfig.Options.Password,//Password
                HostName = FakeConfig.Options.HostName,//rabbitmq ip
                VirtualHost = FakeConfig.Options.VirtualHost
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var sendBytes = this.serializer.Serialize(msg);
            for (int i = 0; i < count; i++)
            {
                channel.BasicPublish(this.exchange, this.exchange, null, sendBytes);
            }
        }

        private void ThenProcess(EventModel m)
        {
            Task.Delay(5000).GetAwaiter().GetResult();
            Assert.Equal(this.model.TypeCode, m.TypeCode);
            Assert.Equal(this.model.Version, m.Version);
            ((TestEvent)m.Event).Valid((TestEvent)this.model.Event);
        }
    }
}
