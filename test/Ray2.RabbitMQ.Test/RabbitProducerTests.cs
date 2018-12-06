using Ray2.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using Orleans.Runtime;
using Xunit;
using TestStack.BDDfy;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ.Test
{
    public class RabbitProducerTests
    {
        private readonly RabbitProducer producer;
        private readonly ISerializer serializer;
        private string exchange = "RabbitProducerTests";
        private string routingKey = "RabbitProducerTests";
        private string queue = "RabbitProducerTests";
        private PublishMessage message;
        private bool IsPublish;
        public RabbitProducerTests()
        {
            IServiceProvider serviceProvider = FakeConfig.BuildServiceProvider();
            serializer = serviceProvider.GetRequiredServiceByName<ISerializer>(FakeConfig.Options.SerializationType);
            producer = new RabbitProducer(serviceProvider, FakeConfig.ProviderName, serializer);
        }

        [Fact]
        public void should_Publish_Normal()
        {
            PublishMessage msg = new PublishMessage()
            {
                Data = serializer.Serialize("test"),
                TypeCode = "test",
                Version = 1
            };

            this.When(f => f.WhenPublish(msg))
                .When(f => f.WhenReceive())
                .Then(f => f.ThenReceiveEqual(msg))
                .BDDfy();
        }

        [Fact]
        public void should_Publish_CloseConn()
        {
            PublishMessage msg = new PublishMessage()
            {
                Data = serializer.Serialize("test"),
                TypeCode = "test",
                Version = 1
            };

            this.When(f => f.WhenPublishClose(msg))
              .Then(f => f.ThenFailure())
              .BDDfy();
        }

        private void WhenPublishClose(PublishMessage msg)
        {
            this.producer.Close();
            this.IsPublish = this.producer.Publish(this.exchange, this.routingKey, msg).GetAwaiter().GetResult();

        }
        private void WhenPublish(PublishMessage msg)
        {
            this.IsPublish = this.producer.Publish(this.exchange, this.routingKey, msg).GetAwaiter().GetResult();
        }

        private void WhenReceive()
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
            EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
            channel.ExchangeDeclare(this.exchange, ExchangeType.Direct, true);
            channel.QueueDeclare(this.queue, false, false, false, null);
            channel.QueueBind(this.queue, this.exchange, this.routingKey, null);
            channel.BasicConsume(this.queue, false, consumer);
            consumer.Received += (ch, ea) =>
            {
                channel.BasicAck(ea.DeliveryTag, false);
                message = serializer.Deserialize<PublishMessage>(ea.Body);
            };
        }

        private void ThenReceiveEqual(PublishMessage msg)
        {
            int count = 0;
            while (true)
            {
                if (message == null)
                {
                    if (count > 20)
                        throw new Exception("Data receive failed");
                    count++;
                    Task.Delay(1000).GetAwaiter().GetResult();
                    return;
                }
                Assert.True(IsPublish);
                Assert.Equal(message.Data, msg.Data);
                Assert.Equal(message.TypeCode, msg.TypeCode);
                Assert.Equal(message.Version, msg.Version);
            }

        }
        private void ThenFailure()
        {
            Assert.False(IsPublish);
        }


    }
}
