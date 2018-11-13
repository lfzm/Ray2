using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ray2.Test.Model
{
    public class TestModel
    {
        public string Test { get; set; }

        public  void Valid(TestModel state)
        {
            Assert.Equal(state.Test, this.Test);
        }
        }
}
