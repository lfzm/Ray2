using Ray2.Serialization;
using System;
using TestStack.BDDfy;
using Xunit;

namespace Ray2.PostgreSQL.Test
{
    public class PostgreSqlTableStorageTests
    {
        private PostgreSqlTableStorage storage;
        public PostgreSqlTableStorageTests()
        {
            this.Given_Build_PostgreSqlTableStorage();
        }

        [Fact]
        public void should_CreateEventTable_Success()
        {
            this.When(f => f.When_CreateEventTable("es_test",123))
                .Then(f => f.ThenSuccess())
                .BDDfy();
        }
        [Fact]
        public void should_CreateEventTable_Existed()
        {
            string tableName = "es_test_existed";
            this.When(f => f.When_CreateEventTable(tableName, Guid.NewGuid()))
                .When(f => f.When_CreateEventTable(tableName, Guid.NewGuid()))
                .Then(f => f.ThenSuccess())
                .BDDfy();
        }
        [Fact]
        public void should_CreateEventTable_Repeat()
        {
            string tableName = "es_test_repeat";
            this
                .When(f => f.When_CreateEventTable(tableName,Guid.NewGuid()))
                .Given(f => f.Given_Build_PostgreSqlTableStorage())
                .When(f => f.When_CreateEventTable(tableName, Guid.NewGuid()))
                .Then(f => f.ThenSuccess())
                .BDDfy();

        }
        [Fact]
        public void should_CreateStateTable_Success()
        {
            this.When(f => f.When_CreateStateTable("st_test","abc"))
                .Then(f => f.ThenSuccess())
                .BDDfy();
        }
        [Fact]
        public void should_CreateStateTable_Existed()
        {
            string tableName = "st_test_existed";
            this.When(f => f.When_CreateStateTable(tableName,DateTime.Now.Ticks))
                .When(f => f.When_CreateStateTable(tableName, DateTime.Now.Ticks))
                .Then(f => f.ThenSuccess())
                .BDDfy();
        }
        [Fact]
        public void should_CreateStateTable_Repeat()
        {
            string tableName = "st_test_repeat";
            this.When(f => f.When_CreateStateTable(tableName,11))
                .Given(f => f.Given_Build_PostgreSqlTableStorage())
                .When(f => f.When_CreateStateTable(tableName,11))
                .Then(f => f.ThenSuccess())
                .BDDfy();
        }

        private void Given_Build_PostgreSqlTableStorage()
        {
            IServiceProvider serviceProvider = FakeConfig.BuildServiceProvider(SerializationType.String);
            storage = new PostgreSqlTableStorage(serviceProvider, FakeConfig.ProviderName);
        }
        private void When_CreateEventTable(string name,object stateId)
        {
            this.storage.CreateEventTable(name, stateId).GetAwaiter().GetResult();
        }
        private void When_CreateStateTable(string name, object stateId)
        {
            this.storage.CreateStateTable(name, stateId).GetAwaiter().GetResult();
        }

        private void ThenSuccess()
        {
            Assert.True(true);
        }
    }
}
