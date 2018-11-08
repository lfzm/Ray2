using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
