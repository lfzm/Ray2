using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ray2.Serialization;
using Ray2.Storage;
using System;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public class PostgreSqlStateStorage : IStateStorage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        private readonly IPostgreSqlTableStorage _tableStorage;
        private readonly PostgreSqlOptions _options;
        private readonly string ProviderName;

        public PostgreSqlStateStorage(IServiceProvider serviceProvider, string name)
        {
            this.ProviderName = name;
            this._serviceProvider = serviceProvider;
            this._tableStorage = serviceProvider.GetRequiredService<IPostgreSqlTableStorage>();
            this._options = serviceProvider.GetRequiredService<IOptionsSnapshot<PostgreSqlOptions>>().Get(name);
            this._logger = serviceProvider.GetRequiredService<ILogger<PostgreSqlStateStorage>>();
            this._serializer = serviceProvider.GetRequiredService<ISerializer>();
        }

        public async Task<bool> DeleteAsync(string tableName, object stateId)
        {
            using (var db = PostgreSqlDbContext.Create(this._options))
            {
                string sql = $"DELETE FROM {tableName} where stateid=@StateId";
                return await db.ExecuteAsync(sql, new { StateId = stateId.ToString() }) > 0;
            }
        }
        public async Task<bool> InsertAsync<TState>(string tableName, object stateId, TState state) where TState : IState, new()
        {
            await this._tableStorage.CreateStateTable(tableName, stateId);
            using (var db = PostgreSqlDbContext.Create(this._options))
            {
                string sql = $"INSERT into {tableName}(stateid,data)VALUES(@StateId,@Data)";
                var data = this._serializer.Serialize(state);
                return await db.ExecuteAsync(sql, new { StateId = stateId.ToString(), Data = data }) > 0;
            }
        }
        public async Task<TState> ReadAsync<TState>(string tableName, object stateId) where TState : IState, new()
        {
            using (var db = PostgreSqlDbContext.Create(this._options))
            {
                string sql = $"select data FROM {tableName} where stateid=@StateId";
                var data = await db.ExecuteScalarAsync<byte[]>(sql, new { StateId = stateId.ToString() });
                return this._serializer.Deserialize<TState>(data);
            }
        }
        public async Task<bool> UpdateAsync<TState>(string tableName, object stateId, TState state) where TState : IState, new()
        {
            using (var db = PostgreSqlDbContext.Create(this._options))
            {
                string sql = $"Update {tableName} set data=@Data where stateid=@StateId";
                var data = this._serializer.Serialize(state);
                return await db.ExecuteAsync(sql, new { StateId = stateId.ToString(), Data = data }) > 0;
            }
        }
    }
}
