using Ray2.EventSource;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using TestStack.BDDfy;
using Orleans.Runtime;

namespace Ray2.PostgreSQL.Test
{
    public class PostgreSqlEventStorageTests
    {
        private IServiceProvider serviceProvider;
        private PostgreSqlEventStorage storage;
        private string TableName;
        private string EventSourceName;
        private EventStorageBufferWrap eventWrap;
        public PostgreSqlEventStorageTests()
        {
            this.EventSourceName = "Test";
            this.TableName = "es_storage";
            this.serviceProvider = FakeConfig.BuildServiceProvider();
            this.storage = new PostgreSqlEventStorage(serviceProvider, FakeConfig.Options, FakeConfig.ProviderName, this.TableName);
        }

        [Fact]
        public void should_Save_Success()
        {
            var e = TestEvent.Create(1);
            e.StateId = 1000;
            EventSingleStorageModel model = new EventSingleStorageModel(e.StateId, e, this.EventSourceName, this.TableName);

            eventWrap = new EventStorageBufferWrap(model);
            List<EventStorageBufferWrap> events = new List<EventStorageBufferWrap>
            {
                eventWrap
            };

            this
                .Given(f => f.Given_CreateTable())
                .When(f => f.When_Save(events))
                .Then(f => f.Then_Save_Sccess())
                .BDDfy();

        }

        [Fact]
        public void should_SQLSave_SimpleSuccess()
        {

        }

        [Fact]
        public void should_BinarySave_SimpleSuccess()
        {

        }

        [Fact]
        public void should_SQLSave_BatchSuccess()
        {

        }

        [Fact]
        public void should_BinarySave_BatchSuccess()
        {

        }


        [Fact]
        public void should_SQLSave_Unique()
        {

        }

        [Fact]
        public void should_BinarySave_Unique()
        {

        }

        [Fact]
        public void should_SQLSave_UniqueIndex()
        {

        }

        [Fact]
        public void should_BinarySave_UniqueIndex()
        {

        }


        [Fact]
        public void should_SQLSave_BatchException()
        {

        }

        [Fact]
        public void should_BinarySave_BatchException()
        {

        }

        private void Given_CreateTable()
        {
            var tableStorage = serviceProvider.GetRequiredServiceByName<IPostgreSqlTableStorage>(FakeConfig.ProviderName);
            tableStorage.CreateEventTable(this.TableName, 1);
        }
        private void When_Save(List<EventStorageBufferWrap> list)
        {
            this.storage.SaveAsync(list).GetAwaiter().GetResult();
        }

        private void Then_Save_Sccess()
        {
            var isSuccess = this.eventWrap.TaskSource.Task.GetAwaiter().GetResult();
            Assert.True(isSuccess);
        }

    }

}
