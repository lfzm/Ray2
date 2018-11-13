using Orleans.Runtime;
using Ray2.EventProcess;
using System;
using System.Runtime.Serialization;
using TestStack.BDDfy;
using Xunit;

namespace Ray2.PostgreSQL.Test
{
    public class PostgreSqlStateStorageTests
    {
        private readonly IServiceProvider serviceProvider;
        private PostgreSqlStateStorage storage;
        private string TableName;
        private TestState ReadState;
        private bool IsSuccess;
        private Exception exception;
        public PostgreSqlStateStorageTests()
        {
            this.TableName = "st_storage";
            serviceProvider = FakeConfig.BuildServiceProvider();
            storage = new PostgreSqlStateStorage(serviceProvider, FakeConfig.Options, FakeConfig.ProviderName, this.TableName);
        }
        [Fact]
        public void should_Insert_Success()
        {
            TestState state = new TestState { StateId = 100 };
            state.Test = DateTime.Now.Ticks.ToString();
            this.Given(f => f.Givent_PlayerEvent(state, 1))
                .Given(f=>f.Givent_CreateTable())
                .When(f => f.When_Delete(state.StateId))
                .When(f => f.When_Insert(state))
                .When(f => f.When_Read(state.StateId))
                .Then(f => f.ThenModifySuccess(state))
                .BDDfy();
        }
        [Fact]
        public void should_Insert_Repeat()
        {
            TestState state = new TestState { StateId = 101 };

            this.Given(f => f.Givent_PlayerEvent(state, 1))
                .Given(f => f.Givent_CreateTable())
                .When(f => f.When_Insert(state))
                .When(f => f.When_Insert(state))
                .Then(f => f.ThenExecuteInsert_UniqueViolation())
                .BDDfy();
        }
        [Fact]
        public void should_Delete_Success()
        {
            TestState state = new TestState { StateId = 102 };
            state.Test = DateTime.Now.Ticks.ToString();
            this.Given(f => f.Givent_PlayerEvent(state, 1))
                .Given(f => f.Givent_CreateTable())
                .When(f => f.When_Insert(state))
                .When(f => f.When_Delete(state.StateId))
                .When(f => f.When_Read(state.StateId))
                .Then(f => f.ThenReadNull())
                .BDDfy();
        }
        [Fact]
        public void should_Update_Success()
        {
            TestState state = new TestState { StateId = 103 };
            state.Test = DateTime.Now.Ticks.ToString();
            this.Given(f => f.Givent_PlayerEvent(state, 1))
                .Given(f => f.Givent_CreateTable())
                .When(f => f.When_Delete(state.StateId))
                .When(f => f.When_Insert(state))
                .Given(f => f.Givent_PlayerEvent(state, 2))
                .When(f => f.When_Update(state))
                .When(f => f.When_Read(state.StateId))
                .Then(f => f.ThenModifySuccess(state))
                .BDDfy();
        }
        private void Givent_PlayerEvent(TestState state, long v)
        {
            var e = TestEvent.Create(v);
            state.Player(e);
        }
        private void Givent_CreateTable()
        {
            var tableStorage = serviceProvider.GetRequiredServiceByName<IPostgreSqlTableStorage>(FakeConfig.ProviderName);
            tableStorage.CreateStateTable(this.TableName, 1);
        }
        private void When_Insert(TestState state)
        {
            try
            {
                IsSuccess = storage.InsertAsync(state.StateId, state).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }
        private void When_Update(TestState state)
        {
            IsSuccess = storage.UpdateAsync( state.StateId, state).GetAwaiter().GetResult();
        }
        private void When_Delete(int stateId)
        {
            IsSuccess = storage.DeleteAsync( stateId).GetAwaiter().GetResult();
        }
        private void When_Read(int stateId)
        {
            ReadState = storage.ReadAsync<TestState>( stateId).GetAwaiter().GetResult();
        }
        private void ThenModifySuccess(TestState state)
        {
            Assert.Null(exception);
            Assert.NotNull(ReadState);
            ReadState.Valid(state);
        }
        private void ThenExecuteInsert_UniqueViolation()
        {
            Assert.NotNull(exception);
            Assert.Contains("23505:", exception.Message);
        }
        private void ThenReadNull()
        {
            Assert.Null(ReadState);
        }
       
    }


   
}
