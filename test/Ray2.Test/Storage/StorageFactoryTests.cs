using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Runtime;
using Ray2.Configuration;
using Ray2.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace Ray2.Test.Storage
{
    public class StorageFactoryTests
    {
        private readonly IServiceCollection services;
        private readonly Mock<IStorageSharding> storageSharding = new Mock<IStorageSharding>();
        private readonly Mock<IEventStorage> eventStorage = new Mock<IEventStorage>();
        private readonly Mock<IStateStorage> stateStorage = new Mock<IStateStorage>();
        private readonly string providerName = "Default";
        private StorageFactory storageFactory;
        private object obj;
        private string tableName;
        private IList<string> tableNames;
        private string name = "test";
        private List<EventStorageInfo> eventStorages;

        public StorageFactoryTests()
        {
            services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>));
            services.AddSingletonNamedService(providerName, (sp, key) => storageSharding.Object);
            services.AddSingletonNamedService(providerName, (sp, key) => eventStorage.Object);
            services.AddSingletonNamedService(providerName, (sp, key) => stateStorage.Object);
        }
        [Fact]
        public void should_GetEventStorageList()
        {
            List<EventStorageInfo> storages = new List<EventStorageInfo>();
            storages.Add(new EventStorageInfo()
            {
                EventSource = name,
                Provider = providerName,
                Storage = eventStorage.Object,
                Tables = new List<string>() { name }
            });
            // Designated storage provider
            this.Given(f => f.GivenInitStorageFactory(new StorageOptions(providerName, "")))
                .When(f => f.WhenGetEventStorageList(name))
                .Then(f => f.ThenEventStorageInfoEqual(storages))
                .BDDfy();

            //Use the hair sharding service
            this.Given(f => f.GivenStorageSharding_GetProviderList(name, StorageType.EventSource, storages))
                .Given(f => f.GivenInitStorageFactory(new StorageOptions(null, providerName)))
                .When(f => f.WhenGetEventStorageList(name))
                .Then(f => f.ThenEventStorageInfoEqual(storages))
                .BDDfy();
        }
        [Fact]
        public void should_GetEventStorage()
        {
            // Designated storage provider
            this.Given(f => f.GivenInitStorageFactory(new StorageOptions(providerName, "")))
                .When(f => f.WhenGetEventStorage(name, name))
                .Then(f => f.ThenNotNull(this.obj))
                .BDDfy();

            //Use the hair sharding service
            this.Given(f => f.GivenStorageSharding_GetProvider(this.name, StorageType.EventSource, name))
                .Given(f => f.GivenInitStorageFactory(new StorageOptions(null, providerName)))
                .When(f => f.WhenGetEventStorage(this.name, name))
                .Then(f => ThenNotNull(this.obj))
                .BDDfy();
        }
        [Fact]
        public void should_GetStateStorage()
        {
            StorageType type = StorageType.EventProcessState;

            // Designated storage provider
            this.Given(f => f.GivenInitStorageFactory(new StorageOptions(providerName, "")))
                .When(f => f.WhenGetStateStorage(this.name, type, name))
                .Then(f => ThenNotNull(this.obj))
                .BDDfy();

            //Use the hair sharding service
            type = StorageType.EventSourceSnapshot;
            this.Given(f => f.GivenStorageSharding_GetProvider(this.name, type, name))
                .Given(f => f.GivenInitStorageFactory(new StorageOptions(null, providerName)))
                .When(f => f.WhenGetStateStorage(this.name, type, name))
                .Then(f => ThenNotNull(this.obj))
                .BDDfy();
        }
        [Fact]
        public void should_GetTableList()
        {
            StorageType type = StorageType.EventProcessState;

            // Designated storage provider
            this.Given(f => f.GivenInitStorageFactory(new StorageOptions(providerName, "")))
                .When(f => f.WhenGetTableList(name, type, name))
                .Then(f => f.ThenListCount(tableNames, new List<string>() { StorageTableNameBuild.BuildTableName(name, type) }))
                .BDDfy();

            //Use the hair sharding service
            List<string> tables = new List<string>()
            {
                "test1","test2","test3","test4","test5","test6",
            };
            IList<string> tables1 = tables.Select(f => StorageTableNameBuild.BuildTableName(f, type)).ToList();
            storageSharding.Setup(f => f.GetTableList(name, type, name, null)).Returns(() =>
            {
                return Task.FromResult(tables);
            });

            this.Given(f => f.GivenInitStorageFactory(new StorageOptions(null, providerName)))
                .When(f => f.WhenGetTableList(name, type, name))
                .Then(f => f.ThenListCount(tableNames, tables1))
                .BDDfy();
        }
        [Fact]
        public void should_GetTable()
        {
            StorageType type = StorageType.EventSource;
            string table1 = StorageTableNameBuild.BuildTableName(name, type);

            // Designated storage provider
            this.Given(f => f.GivenInitStorageFactory(new StorageOptions(providerName, "")))
                .When(f => f.WhenGetTable(name, type, name))
                .Then(f => f.Then<string>(table1, this.tableName))
                .BDDfy();

            //Use the hair sharding service
            storageSharding.Setup(f => f.GetTable(name, type, name)).Returns(Task.FromResult(name));
            this.Given(f => f.GivenInitStorageFactory(new StorageOptions(null, providerName)))
                .When(f => f.WhenGetTable(name, type, name))
                .Then(f => f.Then<string>(table1, this.tableName))
                .BDDfy();
        }
        [Fact]
        public void should_GetStorageSharding()
        {
            //No IStorageSharding
            this.Given(f => f.GivenInitStorageFactory(new StorageOptions("", "")))
                .When(f => f.WhenGetStorageSharding())
                .Then(f => f.ThenNull(this.obj))
                .BDDfy();

            // Normal 
            this.Given(f => f.GivenInitStorageFactory(new StorageOptions("", providerName)))
                .When(f => f.WhenGetStorageSharding())
                .Then(f => f.ThenNotNull(this.obj))
                .BDDfy();
        }


        private void GivenInitStorageFactory(StorageOptions options)
        {
            this.storageFactory = new StorageFactory(services.BuildServiceProvider(), options);
        }
        private void GivenStorageSharding_GetProvider(string name, StorageType storageType, string stateKey)
        {
            this.storageSharding.Setup(f => f.GetProvider(name, storageType, stateKey)).Returns(Task.FromResult(this.providerName));
        }
        private void GivenStorageSharding_GetProviderList(string name, StorageType storageType, List<EventStorageInfo> eventStorages)
        {
            this.storageSharding.Setup(f => f.GetProviderList(name, storageType, null)).Returns(Task.FromResult(eventStorages));
        }


        private void WhenGetStorageSharding()
        {
            this.obj = this.storageFactory.GetStorageSharding();
        }
        private void WhenGetTableList(string name, StorageType storageType, string stateKey)
        {
            this.tableNames = this.storageFactory.GetTableList(name, storageType, stateKey, null).GetAwaiter().GetResult();
        }
        private void WhenGetTable(string name, StorageType storageType, string stateKey)
        {
            this.tableName = this.storageFactory.GetTable(name, storageType, stateKey).GetAwaiter().GetResult();
        }
        private void WhenGetStateStorage(string name, StorageType storageType, string stateKey)
        {
            this.obj = this.storageFactory.GetStateStorage(name, storageType, stateKey).GetAwaiter().GetResult();
        }
        private void WhenGetEventStorage(string name, string stateKey)
        {
            this.obj = this.storageFactory.GetEventStorage(name, stateKey);
        }
        private void WhenGetEventStorageList(string name)
        {
            this.eventStorages = this.storageFactory.GetEventStorageList(name, null).GetAwaiter().GetResult();
        }

        private void ThenListCount(IList<string> expectedList, IList<string> actualList)
        {
            Assert.Equal(expectedList.Count, actualList.Count);
            var list = expectedList.Select(f =>
              {
                  return actualList.Contains(f);
              }).ToList();
            foreach (var item in list)
            {
                Assert.True(item);
            }
        }
        private void Then<T>(T expected, T actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }
        private void ThenNotNull(object obj)
        {
            Assert.NotNull(obj);
        }
        private void ThenNull(object obj)
        {
            Assert.Null(obj);
        }
        private void ThenEventStorageInfoEqual(List<EventStorageInfo> infos)
        {
            Assert.Equal(infos.Count, eventStorages.Count);

        }
    }
}
