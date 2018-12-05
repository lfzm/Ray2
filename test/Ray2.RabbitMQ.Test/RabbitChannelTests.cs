using System;
using TestStack.BDDfy;
using Xunit;

namespace Ray2.RabbitMQ.Test
{
    public class RabbitChannelTests
    {
        private readonly RabbitChannel channel;
        private readonly IServiceProvider serviceProvider;
        private readonly IRabbitConnection connection;
        private bool IsOpen;
        private uint MessageCount;
        public RabbitChannelTests()
        {
            this.serviceProvider = FakeConfig.BuildServiceProvider();
            this.connection = new RabbitConnection(this.serviceProvider,FakeConfig.Options);
            this.channel = new RabbitChannel(this.connection);
        }

        [Fact]
        public void should_IsOpen_Success()
        {
            this.When(f => f.WhenIsOpen())
                .Then(f => f.ThenIsOpenSuccess())
                .BDDfy();
        }
        [Fact]
        public void should_IsOpen_Closed()
        {
            this.When(f => f.WhenClose())
                .When(f => f.WhenIsOpen())
                .Then(f => f.ThenIsOpenClose())
                .BDDfy();

        }
        [Fact]
        public void should_Close()
        {
            this.When(f => f.WhenClose())
                .Then(f => f.ThenSuccess())
                .BDDfy();
        }
        [Fact]
        public void should_MessageCount_Success()
        {
            this.Given(f=>f.GrainCreateModel())
                .When(f => f.WhenMessageCount())
                .Then(f => f.ThenSuccess())
                .BDDfy();
        }

        private void GrainCreateModel()
        {
            this.channel.Model.QueueDeclare("RabbitChannelTests", false, false, false, null);
        }

        private void WhenClose()
        {
            this.channel.Close();
        }

        private void WhenIsOpen()
        {
            this.IsOpen = this.channel.IsOpen();
        }

        private void WhenMessageCount()
        {
            this.MessageCount = this.channel.MessageCount("RabbitChannelTests");
        }

        private void ThenSuccess()
        {
            Assert.True(true);
        }

        private void ThenIsOpenSuccess()
        {
            Assert.True(this.IsOpen);
        }

        private void ThenIsOpenClose()
        {
            Assert.False(this.IsOpen);
        }

        
    }
}
