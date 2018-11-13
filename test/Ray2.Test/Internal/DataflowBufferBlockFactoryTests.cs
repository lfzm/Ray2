using Ray2.Internal;
using Ray2.Test.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace Ray2.Test.Internal
{
    public class DataflowBufferBlockFactoryTests
    {
        private DataflowBufferBlockFactory dataflowBufferBlockFactory;
        private List<TestDataflowWrap> Models;
        private TestDataflowWrap Model1;

        public DataflowBufferBlockFactoryTests()
        {
            dataflowBufferBlockFactory = new DataflowBufferBlockFactory();
            Models = new List<TestDataflowWrap>();
        }

        [Fact]
        public void should_Create_Success()
        {
            TestDataflowWrap model1 = new TestDataflowWrap { Test = "1" };
            TestDataflowWrap model2 = new TestDataflowWrap { Test = "2" };
            TestDataflowWrap model3 = new TestDataflowWrap { Test = "3" };

            var dataflowBufferBlock1 = this.dataflowBufferBlockFactory.Create<TestDataflowWrap>("test", TriggerProcessor);
            var dataflowBufferBlock2 = this.dataflowBufferBlockFactory.Create<TestDataflowWrap>("test", TriggerProcessor);
            var dataflowBufferBlock3 = this.dataflowBufferBlockFactory.Create<TestDataflowWrap>("test1", TriggerProcessor1);

            var tasks = new Task<bool>[3];
            tasks[0] = dataflowBufferBlock1.SendAsync(model1);
            tasks[1] = dataflowBufferBlock2.SendAsync(model2);
            tasks[2] = dataflowBufferBlock3.SendAsync(model3);
            Task.WhenAll(tasks).Wait();


            foreach (var task in tasks)
            {
                Assert.True(task.Result);
            }
            for (int i = 0; i < Models.Count(); i++)
            {
                if (Models[i].Test == "1")
                {
                    Models[i].Valid(model1);
                }
                else if (Models[i].Test == "2")
                {
                    Models[i].Valid(model2);
                }
                else
                {
                    throw new Exception("eventy bu yiz");
                }
            }
            Model1.Valid(model3);

        }


        private Task TriggerProcessor(BufferBlock<TestDataflowWrap> bufferBlock)
        {
            while (bufferBlock.TryReceive(out TestDataflowWrap model))
            {
                model.TaskSource.SetResult(true);
                Models.Add(model);
            }
            return Task.CompletedTask;
        }

        private Task TriggerProcessor1(BufferBlock<TestDataflowWrap> bufferBlock)
        {
            while (bufferBlock.TryReceive(out TestDataflowWrap model))
            {
                model.TaskSource.SetResult(true);
                this.Model1 = model;
            }
            return Task.CompletedTask;
        }
    }
}
