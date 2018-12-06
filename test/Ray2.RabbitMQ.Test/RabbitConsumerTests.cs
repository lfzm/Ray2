using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using TestStack.BDDfy;
using Moq;
using Orleans.Runtime;
using Ray2.Serialization;
using Ray2.EventProcess;
using Ray2.RabbitMQ.Configuration;
using RabbitMQ.Client;
using Ray2.EventSource;
using Ray2.RabbitMQ.Test.Model;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ.Test
{
    public class RabbitConsumerTests
    {
        private readonly ISerializer serializer;
        private readonly IServiceProvider serviceProvider;
        private readonly Mock<IEventProcessor> eventProcessor;
        private readonly RabbitConsumeOptions consumeOptions;
        private string queue = "RabbitConsumerTests";
        private string exchange = "RabbitConsumerTests";
        private RabbitConsumer consumer;
        private EventModel model;
        public RabbitConsumerTests()
        {
            serviceProvider = FakeConfig.BuildServiceProvider();
            serializer = serviceProvider.GetRequiredServiceByName<ISerializer>(FakeConfig.Options.SerializationType);
            this.consumer = new RabbitConsumer(serviceProvider, FakeConfig.ProviderName, serializer);
            this.eventProcessor = new Mock<IEventProcessor>();
            this.consumeOptions = new RabbitConsumeOptions();
        }
        [Fact]
        public void should_Subscribe_Normal()
        {
            EventModel model = new EventModel(TestEvent.Create(1));
            PublishMessage msg = new PublishMessage(model, this.serializer);
            this.Given(f => f.GivenInitProcessor())
                .When(f => f.WhenSubscribe("sub"))
                .When(f => f.WhenPublish(msg, "sub", 1))
                .Then(f => f.ThenProcess(model))
                .BDDfy();
        }
        [Fact]
        private void should_Process_RetryNotice()
        {
            EventModel model = new EventModel(TestEvent.Create(1));
            this.consumeOptions.NoticeRetriesCount = 3;
            this.Given(f => f.GivenInitProcessorFailure())
                .When(f => f.WhenSubscribe("process"))
                .When(f => f.WhenProcess(model))
                .Then(f => f.ThenSuccess())
                .BDDfy();
        }
        [Fact]
        private void should_IsExpand_True()
        {
            this.consumer = new RabbitConsumer(serviceProvider, FakeConfig.ProviderName, serializer);
            EventModel model = new EventModel(TestEvent.Create(1));
            PublishMessage msg = new PublishMessage(model, this.serializer);
            this.Given(f => f.GivenInitProcessor())
                .When(f => f.WhenSubscribe("expand"))
                .When(f => f.WhenPublish(msg, "expand", 500))
                .When(f => f.WhenIsExpand())
                .Then(f => f.ThenSuccess())
                .BDDfy();

        }

        private void GivenInitProcessor()
        {
            this.eventProcessor.Setup(f => f.Tell(It.IsNotNull<EventModel>()))
              .Returns(Task.FromResult(true))
              .Callback<EventModel>(f => {
                  this.model = f;
                  Task.Delay(100).GetAwaiter().GetResult();
                  });
        }

        private void GivenInitProcessorFailure()
        {
            this.eventProcessor.Setup(f => f.Tell(It.IsNotNull<EventModel>()))
            .Throws(new Exception("Failure"));
        }

        private void WhenSubscribe(string type)
        {
            consumer.Subscribe(this.queue+ type, this.exchange+ type, this.eventProcessor.Object, consumeOptions).GetAwaiter().GetResult();
        }

        private void WhenIsExpand()
        {
            Task.Delay(1000).GetAwaiter().GetResult();
            var isExpand = this.consumer.IsExpand();
            Assert.True(isExpand);
        }
        private void WhenProcess(EventModel m)
        {
            var task = this.consumer.Process(m, 0);
            //Three retry times take about 6.5 seconds, and if it is exceeded, it will fail.
            Assert.True(task.Wait(6500));
        }
        private void WhenPublish(PublishMessage msg,string type, int count = 1)
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
                channel.BasicPublish(this.exchange+ type, this.exchange+ type, null, sendBytes);
            }
        }

        private void ThenProcess(EventModel m)
        {
            Task.Delay(5000).GetAwaiter().GetResult();
            Assert.Equal(this.model.TypeCode, m.TypeCode);
            Assert.Equal(this.model.Version, m.Version);
            ((TestEvent)m.Event).Valid((TestEvent)this.model.Event);
        }

        private void ThenSuccess()
        {
            Assert.True(true);
        }
    }
}
