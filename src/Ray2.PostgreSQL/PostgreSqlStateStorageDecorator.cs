using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Ray2.Storage;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public class PostgreSqlStateStorageDecorator : IStateStorage
    {
        private readonly ConcurrentDictionary<string, IPostgreSqlStateStorage> storageList = new ConcurrentDictionary<string, IPostgreSqlStateStorage>();
        private readonly IServiceProvider _serviceProvider;
        private readonly IPostgreSqlTableStorage _tableStorage;
        private readonly PostgreSqlOptions _options;
        private readonly string _providerName;

        public PostgreSqlStateStorageDecorator(IServiceProvider serviceProvider, string name)
        {
            this._providerName = name;
            this._serviceProvider = serviceProvider;
            this._tableStorage = serviceProvider.GetRequiredServiceByName<IPostgreSqlTableStorage>(name);
            this._options = serviceProvider.GetRequiredService<IOptionsSnapshot<PostgreSqlOptions>>().Get(name);
        }
        public Task<bool> DeleteAsync(string tableName, object stateId)
        {
            var stotage = this.GetStorage(tableName, stateId);
            return stotage.DeleteAsync(stateId);
        }

        public Task<bool> InsertAsync<TState>(string tableName, object stateId, TState state) where TState : IState, new()
        {
            var stotage = this.GetStorage(tableName, stateId);
            return stotage.InsertAsync<TState>(stateId, state);
        }

        public Task<TState> ReadAsync<TState>(string tableName, object stateId) where TState : IState, new()
        {
            var stotage = this.GetStorage(tableName, stateId);
            return stotage.ReadAsync<TState>(stateId);
        }

        public Task<bool> UpdateAsync<TState>(string tableName, object stateId, TState state) where TState : IState, new()
        {
            var stotage = this.GetStorage(tableName, stateId);
            return stotage.UpdateAsync<TState>(stateId, state);
        }
        private IPostgreSqlStateStorage GetStorage(string tableName, object stateId)
        {
            return storageList.GetOrAdd(tableName, (key) =>
            {
                this._tableStorage.CreateStateTable(tableName, stateId).GetAwaiter();
                return new PostgreSqlStateStorage(this._serviceProvider, _options, this._providerName, key);
            });
        }
    }
}
