using Ray2.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Ray2.Test.Model
{
    public class TestDataflowWrap: IDataflowBufferWrap
    {
        public TestDataflowWrap()
        {
            this.TaskSource = new TaskCompletionSource<bool>();
        }
        public string Test { get; set; }

        public TaskCompletionSource<bool> TaskSource { get; }

        public  void Valid(TestDataflowWrap state)
        {
            Assert.Equal(state.Test, this.Test);
        }
        }
}
