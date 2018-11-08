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

        public static void  Valid(TestState state)
        {
            Assert.Equal(state.StateId, state.StateId);
            Assert.Equal(state.Test, state.Test);
            Assert.Equal(state.TypeCode, state.TypeCode);
            Assert.Equal(state.Version, state.Version);
            Assert.Equal(state.VersionTime, state.VersionTime);
        }
    }
}
