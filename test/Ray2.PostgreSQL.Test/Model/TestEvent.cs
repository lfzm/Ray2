using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ray2.PostgreSQL.Test
{
    public class TestEvent : Event<int>
    {
        public static TestEvent Create(long v)
        {
            return new TestEvent()
            {
                StateId = 100,
                Version = v
            };
        }

        public void Valid(TestEvent state)
        {
            Assert.Equal(state.StateId, this.StateId);
            Assert.Equal(state.Version, this.Version);
            Assert.Equal(state.TypeCode, this.TypeCode);
            Assert.Equal(state.Version, this.Version);
            Assert.Equal(state.Timestamp, this.Timestamp);
            Assert.Equal(state.RelationEvent, this.RelationEvent);
        }
    }
}
