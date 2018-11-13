using Ray2.Internal;
using Ray2.Test.Model;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestStack.BDDfy;
using Xunit;

namespace Ray2.Test.Internal
{
    public class DataflowBufferBlockTests
    {
        private DataflowBufferBlock<TestModel> dataflowBufferBlock;
        private TestModel Model;
        private bool IsSuccess;

        public DataflowBufferBlockTests()
        {
            dataflowBufferBlock = new DataflowBufferBlock<TestModel>(this.TriggerProcessor);
        }

        [Fact]
        public void should_Send_Success()
        {
            TestModel model = new TestModel { Test = "should_Send_Success" };

            this.When(f => f.WhenSendAsync(model))
                .When(f=>f.WhenCanceledBuffer())
                .Then(f => f.ThenSuccess(model))
                .BDDfy();
        }

        private void WhenSendAsync(TestModel model)
        {
            IsSuccess = this.dataflowBufferBlock.SendAsync(model).GetAwaiter().GetResult();
        }


        private void WhenCanceledBuffer()
        {
            Task.Delay(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
            this.dataflowBufferBlock.Canceled();
        }

        private void ThenSuccess(TestModel model)
        {
            Assert.True(IsSuccess);
            this.Model.Valid(model);
        }

        private Task TriggerProcessor(BufferBlock<TestModel> bufferBlock)
        {
            while (bufferBlock.TryReceive(out TestModel model))
            {
                this.Model = model;
            }
            return Task.CompletedTask;
        }
    }
}
