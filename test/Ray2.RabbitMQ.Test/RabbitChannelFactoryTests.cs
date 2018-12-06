using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace Ray2.RabbitMQ.Test
{
    public class RabbitChannelFactoryTests
    {
        private readonly RabbitChannelFactory channelFactory;
        public RabbitChannelFactoryTests()
        {
            IServiceProvider serviceProvider = FakeConfig.BuildServiceProvider();
            this.channelFactory = new RabbitChannelFactory(serviceProvider, FakeConfig.ProviderName);
        }

        [Fact]
        public void should_GetChannel_Success()
        {
            this.When(f => f.WhenGetChannel(1))
                .Then(f => f.ThenSuccess())
                .BDDfy();
        }

        [Fact]
        public void should_GetChannel_100()
        {
            this.When(f => f.WhenGetChannel(100))
            .Then(f => f.ThenSuccess())
            .BDDfy();
        }

        [Fact]
        public void should_GetChannel_Concurrent500()
        {
            this.When(f => f.WhenGetChannel_Concurrent(500))
            .Then(f => f.ThenSuccess())
            .BDDfy();
        }

        private void WhenGetChannel(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var channel = this.channelFactory.GetChannel();
                Assert.True(channel.IsOpen());
                if (i == 50)
                {
                    channel.Connection.Close();
                }
            }
        }

        private void WhenGetChannel_Concurrent(int count)
        {
            var tasks = new Task[count];
            Parallel.For(0, count, (i) =>
            {
                var channel = this.channelFactory.GetChannel();
                Assert.True(channel.IsOpen());
                tasks[i] = Task.CompletedTask;
            });
            Task.WaitAll(tasks);
        }

        private void ThenSuccess()
        {
            Assert.True(true);
        }
    }
}
