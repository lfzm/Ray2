using Ray2.RabbitMQ.Configuration;
using System;
using TestStack.BDDfy;
using Xunit;

namespace Ray2.RabbitMQ.Test
{
    public class RabbitConnectionTests
    {
        private readonly RabbitOptions options;
        private readonly IServiceProvider serviceProvider;
        private readonly RabbitConnection connection;
        private Exception exception;
        private IRabbitChannel channel;

        public RabbitConnectionTests()
        {
            this.options = FakeConfig.Options;
            this.serviceProvider = FakeConfig.BuildServiceProvider();
            this.connection = new RabbitConnection(this.serviceProvider, this.options);
        }

        [Fact]
        public void should_createChannel_Success()
        {
            this.When(f => f.WhenCreateChannel())
                .Then(f => f.ThenChannelIsOpen())
                .BDDfy();
        }

        [Fact]
        public void should_createChannel_ConnectionClosed()
        {
            this.Given(f => f.WhenCloseCoonnection())
                .When(f => f.WhenCreateChannel_ConnectionClosed())
                .Then(f => f.ThenException())
                .BDDfy();
        }

        [Fact]
        public void should_Close()
        {
            this.When(f => f.WhenCloseCoonnection())
               .Then(f => f.ThenScuucess())
               .BDDfy();
        }

        private void WhenCloseCoonnection()
        {
            this.connection.Close();
        }

        private void WhenCreateChannel()
        {
            channel=this.connection.CreateChannel();
        }

        private void WhenCreateChannel_ConnectionClosed()
        {
            try
            {
                channel= this.connection.CreateChannel();
            }
            catch (Exception ex)
            {
                this.exception = ex;
            }
        }

        private void ThenException()
        {
            Assert.NotNull(this.exception);
        }
        private void ThenScuucess()
        {
            Assert.True(true);
        }

        private void ThenChannelIsOpen()
        {
            Assert.True(this.channel.IsOpen());
        }
    }
}
