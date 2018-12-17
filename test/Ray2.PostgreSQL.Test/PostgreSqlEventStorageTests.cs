using Ray2.EventSource;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using TestStack.BDDfy;
using Orleans.Runtime;
using System.Linq;

namespace Ray2.PostgreSQL.Test
{
    public class PostgreSqlEventStorageTests
    {
        private IServiceProvider serviceProvider;
        private PostgreSqlEventStorage storage;
        private string TableName;
        private string EventSourceName;
        private EventStorageBufferWrap eventWrap;
        private List<EventStorageBufferWrap> eventWraps;
        private EventModel eventModel;
        public PostgreSqlEventStorageTests()
        {
            this.EventSourceName = "Test";
            this.TableName = "es_storage";
            this.serviceProvider = FakeConfig.BuildServiceProvider();
            this.storage = new PostgreSqlEventStorage(serviceProvider, FakeConfig.Options, FakeConfig.ProviderName, this.TableName);
        }

        [Fact]
        public void should_Save_SingleSuccess()
        {
            var e = TestEvent.Create(1);
            e.StateId = 1000;
            EventStorageModel model = new EventStorageModel(e.StateId, e, this.EventSourceName, this.TableName);

            eventWrap = new EventStorageBufferWrap(model);
            List<EventStorageBufferWrap> eventWraps = new List<EventStorageBufferWrap>
            {
                eventWrap
            };

            this
                .Given(f => f.Given_CreateTable())
                .When(f => f.When_Save(eventWraps))
                .When(f => f.When_Get(e.StateId, e.Version))
                .Then(f => f.Then_Save_SingleSuccess(e))
                .BDDfy();
        }

        [Fact]
        public void should_Save_MickleSuccess()
        {
            eventWraps = new List<EventStorageBufferWrap>();
            for (int i = 0; i < 100; i++)
            {
                var e = TestEvent.Create(1);
                e.StateId = 1001 + i;
                EventStorageModel model = new EventStorageModel(e.StateId, e, this.EventSourceName, this.TableName);
                eventWraps.Add(new EventStorageBufferWrap(model));
            }

            this.Given(f => f.Given_CreateTable())
                .When(f => f.When_Save(eventWraps))
                .Then(f => f.Then_Save_MickleSuccess())
                .BDDfy();
        }

        [Fact]
        public void should_Save_UniqueConstraint()
        {
            eventWraps = new List<EventStorageBufferWrap>();
            for (int i = 0; i < 3; i++)
            {
                var e = TestEvent.Create(i);
                if (i == 2)
                    e.Version = 1;
                e.StateId = 2000;
                EventStorageModel model = new EventStorageModel(e.StateId, e, this.EventSourceName, this.TableName);
                eventWraps.Add(new EventStorageBufferWrap(model));
            }

            this.Given(f => f.Given_CreateTable())
                .Then(f => Then_Save_UniqueConstraint(eventWraps))
                .BDDfy();
        }

        [Fact]
        public void should_Save_IndexConstraint()
        {
            eventWraps = new List<EventStorageBufferWrap>();
            for (int i = 0; i < 3; i++)
            {
                var e = TestEvent.Create(i);
                if (i == 2)
                    e.RelationEvent = "RelationEvent1";
                else
                    e.RelationEvent = "RelationEvent" + i;

                e.StateId = 3000;
                EventStorageModel model = new EventStorageModel(e.StateId, e, this.EventSourceName, this.TableName);
                eventWraps.Add(new EventStorageBufferWrap(model));
            }

            this.Given(f => f.Given_CreateTable())
                .When(f => f.When_Save(eventWraps))
                .Then(f => Then_Save_IndexConstraint(eventWraps))
                .BDDfy();
        }

        [Fact]
        public void should_GetList()
        {
            eventWraps = new List<EventStorageBufferWrap>();
            for (int i = 0; i < 100; i++)
            {
                var e = TestEvent.Create(1);
                e.StateId = 4000 + i;

                if (i < 50)
                    e.RelationEvent = "Test";
                else
                    e.RelationEvent = "ABC";
                EventStorageModel model = new EventStorageModel(e.StateId, e, this.EventSourceName, this.TableName);
                eventWraps.Add(new EventStorageBufferWrap(model));
            }

            this.Given(f => f.Given_CreateTable())
                .When(f => f.When_Save(eventWraps))
                .Then(f => f.Then_GetList_Success(eventWraps))
                .BDDfy();
        }



        private void Given_CreateTable()
        {
            var tableStorage = serviceProvider.GetRequiredServiceByName<IPostgreSqlTableStorage>(FakeConfig.ProviderName);
            tableStorage.CreateEventTable(this.TableName, 1);
        }

        private void When_Get(object stateId, long version)
        {
            this.eventModel = this.storage.GetAsync(stateId, version).GetAwaiter().GetResult();
        }
        private void When_Save(List<EventStorageBufferWrap> list)
        {
            this.storage.SaveAsync(list).GetAwaiter().GetResult();
        }

        private void Then_Save_SingleSuccess(TestEvent @event)
        {
            var isSuccess = this.eventWrap.TaskSource.Task.GetAwaiter().GetResult();
            Assert.True(isSuccess);
            if (eventModel.Event is TestEvent e)
            {
                e.Valid(@event);
            }
            else
            {
                throw new Exception("eventModel.Data not TestEvent");
            }
        }

        private void Then_Save_MickleSuccess()
        {
            foreach (var wrap in eventWraps)
            {
                var isSuccess = wrap.TaskSource.Task.GetAwaiter().GetResult();
                Assert.True(isSuccess);
            }
        }

        private void Then_Save_IndexConstraint(List<EventStorageBufferWrap> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var isSuccess = list[i].TaskSource.Task.GetAwaiter().GetResult();
                if (i == 2)
                    Assert.False(isSuccess);
                else
                    Assert.True(isSuccess);
            }
        }

        private void Then_Save_UniqueConstraint(List<EventStorageBufferWrap> list)
        {
            var d = new List<EventStorageBufferWrap>() { list[0] };
            this.storage.SaveAsync(d).GetAwaiter().GetResult();
            var isSuccess = list[0].TaskSource.Task.GetAwaiter().GetResult();
            Assert.True(isSuccess);


            d = new List<EventStorageBufferWrap>() { list[1] };
            this.storage.SaveAsync(d).GetAwaiter().GetResult();
            isSuccess = list[1].TaskSource.Task.GetAwaiter().GetResult();
            Assert.True(isSuccess);

            Exception _ex = null;
            try
            {
                d = new List<EventStorageBufferWrap>() { list[2] };
                this.storage.SaveAsync(d).GetAwaiter().GetResult();
                isSuccess = list[2].TaskSource.Task.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _ex = ex;
            }
            Assert.NotNull(_ex);
            Assert.Contains("23505:", _ex.Message);
        }

        private void Then_GetList_Success(List<EventStorageBufferWrap> list)
        {
            EventQueryModel model = new EventQueryModel()
            {
                StateId = 4000
            };
            var datas = this.storage.GetListAsync(model).GetAwaiter().GetResult();
            Assert.True(datas.Count > 0);


            model = new EventQueryModel()
            {
                RelationEvent = "ABC"
            };
            datas = this.storage.GetListAsync(model).GetAwaiter().GetResult();
            Assert.True(datas.Count > 0);

        }


    }
}

