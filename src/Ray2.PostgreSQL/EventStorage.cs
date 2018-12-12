using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Ray2.EventSource;
using Ray2.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public class EventStorage : IEventStorage
    {
        private readonly ConcurrentDictionary<string, IPostgreSqlEventStorage> storageList = new ConcurrentDictionary<string, IPostgreSqlEventStorage>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IPostgreSqlTableStorage _tableStorage;
        private readonly PostgreSqlOptions _options;
        private readonly string _providerName;
        public EventStorage(IServiceProvider serviceProvider, string name)
        {
            this._providerName = name;
            this._serviceProvider = serviceProvider;
            this._tableStorage = serviceProvider.GetRequiredServiceByName<IPostgreSqlTableStorage>(name);
            this._options = serviceProvider.GetRequiredService<IOptionsSnapshot<PostgreSqlOptions>>().Get(name);
        }

        public Task<EventModel> GetAsync(string tableName, object stateId, long version)
        {
            var stotage = this.GetStorage(tableName, stateId);
            return stotage.GetAsync(stateId, version);
        }

        public Task<List<EventModel>> GetListAsync(string tableName, EventQueryModel queryModel)
        {
            var stotage = this.GetStorage(tableName, queryModel.StateId);
            return stotage.GetListAsync(queryModel);
        }

        public Task SaveAsync(List<EventStorageBufferWrap> wrapList)
        {
            Dictionary<string, List<EventStorageBufferWrap>> eventsList = wrapList.GroupBy(f => f.Value.StorageTableName).ToDictionary(x => x.Key, v => v.ToList());
            foreach (var key in eventsList.Keys)
            {
                var events = eventsList[key];
                var stotage = this.GetStorage(key, events.First().Value.GetStateId());
                stotage.SaveAsync(events);
            }
            return Task.CompletedTask;
        }

        public Task<bool> SaveAsync(EventCollectionStorageModel events)
        {
            var stotage = this.GetStorage(events.StorageTableName, events.GetStateId());
            return stotage.SaveAsync(events);
        }

        private IPostgreSqlEventStorage GetStorage(string tableName, object stateId)
        {
            return storageList.GetOrAdd(tableName, (key) =>
            {
                this._tableStorage.CreateEventTable(tableName, stateId);
                return new PostgreSqlEventStorage(this._serviceProvider, _options, this._providerName, key);
            });
        }
    }
}
