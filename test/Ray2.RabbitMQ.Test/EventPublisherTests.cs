using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using TestStack.BDDfy;
using Ray2.EventSource;
using Ray2.RabbitMQ.Test.Model;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ.Test
{
    public class EventPublisherTests
    {
        private readonly EventPublisher publisher;
        private string topic = "EventPublisherTests";
        private bool IsPublish;
        private int producerCount;
        public EventPublisherTests()
        {
            IServiceProvider serviceProvider = FakeConfig.BuildServiceProvider();
            this.publisher = new EventPublisher(serviceProvider, FakeConfig.ProviderName);
        }

        [Fact]
        public void should_PublishSingle_Normal()
        {
            this.When(f => f.WhenPublishSingle())
                .Then(f => f.ThenPublish_Success())
                .BDDfy();
        }
        [Fact]
        public void should_PublishList_Normal()
        {
            this.When(f => f.WhenPublishList())
              .Then(f => f.ThenPublish_Success())
              .BDDfy();
        }

        [Fact]
        public void should_GetProducer_300()
        {
            this.When(f => f.WhenGetProducerList(300))
                .Then(f => f.ThenGetProducer_Success(300))
                .BDDfy();
        }
        [Fact]
        public void should_GetProducer_Concurrent1000()
        {
            this.When(f => f.WhenGetProducerList_Concurrent(1000))
               .Then(f => f.ThenNormal())
               .BDDfy();
        }

        private void WhenPublishSingle()
        {
            EventModel model = new EventModel(TestEvent.Create(100));
            this.IsPublish = this.publisher.Publish(this.topic, model).GetAwaiter().GetResult();
        }

        private void WhenPublishList()
        {
            List<EventModel> models = new List<EventModel>();
            for (int i = 0; i < 10; i++)
            {
                EventModel model = new EventModel(TestEvent.Create(i));
                models.Add(model);
            }
            this.IsPublish = this.publisher.Publish(this.topic, models).GetAwaiter().GetResult();
        }

        private void WhenGetProducerList(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var producer = this.publisher.GetProducer();
                Assert.True(producer.IsAvailable());
                this.publisher.EnqueuePool(producer);
                this.producerCount++;
                if (i == 1)
                {
                    producer.Close();
                }

            }
        }
        private void WhenGetProducerList_Concurrent(int count)
        {
            var tasks = new Task[count];
            Parallel.For(0, count, (i) =>
             {
                 var producer = this.publisher.GetProducer();
                 Assert.True(producer.IsAvailable());
                 Task.Delay(1).GetAwaiter().GetResult();
                 this.publisher.EnqueuePool(producer);
                 tasks[i] = Task.CompletedTask;
             });
            Task.WaitAll(tasks);
        }

        private void ThenPublish_Success()
        {
            Assert.True(this.IsPublish);
        }

        private void ThenGetProducer_Success(int count)
        {
            Assert.Equal(this.producerCount, count);
        }

        private void ThenNormal()
        {
            Assert.True(true);
        }
    }
}
