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
        private DataflowBufferBlock<TestDataflowWrap> dataflowBufferBlock;
        private TestDataflowWrap Model;
        private bool IsSuccess;

        public DataflowBufferBlockTests()
        {
            dataflowBufferBlock = new DataflowBufferBlock<TestDataflowWrap>(this.TriggerProcessor);
        }

        [Fact]
        public void should_Send_Success()
        {
            TestDataflowWrap model = new TestDataflowWrap { Test = "should_Send_Success" };

            this.When(f => f.WhenSendAsync(model))
                .Then(f => f.ThenSuccess(model))
                .BDDfy();
        }

        private void WhenSendAsync(TestDataflowWrap model)
        {
            IsSuccess = this.dataflowBufferBlock.SendAsync(model).GetAwaiter().GetResult();
        }

        private void ThenSuccess(TestDataflowWrap model)
        {
            Assert.True(IsSuccess);
            this.Model.Valid(model);
        }

        private Task TriggerProcessor(BufferBlock<TestDataflowWrap> bufferBlock)
        {
            while (bufferBlock.TryReceive(out TestDataflowWrap model))
            {
                model.TaskSource.SetResult(true);
                this.Model = model;
            }
            return Task.CompletedTask;
        }
    }
}
