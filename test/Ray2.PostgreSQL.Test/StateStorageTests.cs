using System;
using System.Collections.Generic;
using System.Text;
using TestStack.BDDfy;
using Xunit;

namespace Ray2.PostgreSQL.Test
{
    public class StateStorageTests
    {
        private PostgreSQL.StateStorage container;
        private string TableName;
        private TestState selectState;
        private TestState deleteState;

        public StateStorageTests()
        {
            this.TableName = "st_ContainerTest";
            var sp = FakeConfig.BuildServiceProvider();
            container = new PostgreSQL.StateStorage(sp, FakeConfig.ProviderName);
        }
        [Fact]
        public void should_Success()
        {
            TestState state = new TestState()
            {
                StateId = 1,
                Test = DateTime.Now.Ticks.ToString()
            };
            this.When(f => f.WhenInsert(state.StateId, state))
                .When(f => f.WhenUpdate(state.StateId, state))
                .When(f => f.WhenSelect(state.StateId, false))
                .When(f => f.WhenDelete(state.StateId))
                .When(f => f.WhenSelect(state.StateId, true))
                .Then(f => f.ThenSuccess(state))
                .BDDfy();
        }

        private void WhenDelete(object stateId)
        {
            container.DeleteAsync(TableName, stateId).GetAwaiter().GetResult();
        }
        private void WhenInsert(object stateId, TestState state)
        {
            container.InsertAsync(TableName, stateId, state).GetAwaiter().GetResult();
        }
        private void WhenUpdate(object stateId, TestState state)
        {
            state.Test = "abc";
            container.UpdateAsync(TableName, stateId, state).GetAwaiter().GetResult();
        }
        private void WhenSelect(object stateId, bool isdelete)
        {
            if (isdelete)
                deleteState = container.ReadAsync<TestState>(TableName, stateId).GetAwaiter().GetResult();
            else
                selectState = container.ReadAsync<TestState>(TableName, stateId).GetAwaiter().GetResult();
        }

        private void ThenSuccess(TestState state)
        {
            Assert.Null(deleteState);
            Assert.NotNull(selectState);
            selectState.Valid(state);
        }
    }
}
