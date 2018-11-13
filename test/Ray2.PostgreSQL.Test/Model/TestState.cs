using Ray2.EventProcess;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Xunit;

namespace Ray2.PostgreSQL.Test
{
    [DataContract]
    public class TestState : EventProcessState<int>
    {
        [DataMember(Order = 1)]
        public string Test { get; set; }

        public  void  Valid(TestState state)
        {
            Assert.Equal(state.StateId, this.StateId);
            Assert.Equal(state.Test, this.Test);
            Assert.Equal(state.TypeCode, this.TypeCode);
            Assert.Equal(state.Version, this.Version);
            Assert.Equal(state.VersionTime, this.VersionTime);
        }
    }
}
