using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ray2.PostgreSQL.Configuration;
using Ray2.Serialization;
using Ray2.Storage;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public class PostgreSqlStatusStorage : IStateStorage
    {
        private readonly ConcurrentDictionary<string, string> SnapshotTableList = new ConcurrentDictionary<string, string>();
        private readonly IServiceProvider _serviceProvider;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger;
        private readonly IPostgreSqlTableStorage tableStorage;
        private readonly PostgreSqlOptions _options;

        public PostgreSqlStatusStorage(IServiceProvider serviceProvider, PostgreSqlOptions options, IPostgreSqlTableStorage tableStorage)
        {
            this.tableStorage = tableStorage;
            this._serviceProvider = serviceProvider;
            this._options = options;
            this._logger = serviceProvider.GetRequiredService<ILogger<PostgreSqlStatusStorage>>();
            this._serializer = serviceProvider.GetRequiredService<ISerializer>();
        }
   
        public async Task<bool> DeleteAsync(string tableName, object stateId)
        {
            using (var db = this.GetDbContext())
            {
                string sql = $"DELETE FROM {tableName} where stateid=@StateId";
                return await db.ExecuteAsync(sql, new { StateId = stateId.ToString() }) > 0;
            }
        }
        public async Task<bool> InsertAsync<TState>(string tableName, object stateId, TState state) where TState : IState, new()
        {
            await this.tableStorage.CreateSnapshotTable(tableName);
            using (var db = this.GetDbContext())
            {
                string sql = $"INSERT into {tableName}(stateid,data)VALUES(@StateId,@Data)";
                var data = this._serializer.Serialize(state);
                return await db.ExecuteAsync(sql, new { StateId = stateId.ToString(), Data = data }) > 0;
            }
        }
        public async Task<TState> ReadAsync<TState>(string tableName, object stateId) where TState : IState, new()
        {
            using (var db = this.GetDbContext())
            {
                string sql = $"select data FROM {tableName} where stateid=@StateId";
                var data = await db.ExecuteScalarAsync<byte[]>(sql, new { StateId = stateId.ToString() });
                return this._serializer.Deserialize<TState>(data);
            }
        }
        public async Task<bool> UpdateAsync<TState>(string tableName, object stateId, TState state) where TState : IState, new()
        {
            using (var db = this.GetDbContext())
            {
                string sql = $"Update {tableName} set data=@Data where stateid=@StateId";
                var data = this._serializer.Serialize(state);
                return await db.ExecuteAsync(sql, new { StateId = stateId.ToString(), Data = data }) > 0;
            }
        }
     
        private PostgreSqlDbContext GetDbContext()
        {
            return new PostgreSqlDbContext(this._options);
        }

    }
}
