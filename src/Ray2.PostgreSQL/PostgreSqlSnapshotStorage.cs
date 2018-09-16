using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ray2.PostgreSQL.Serialization;
using Ray2.Storage;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public class PostgreSqlSnapshotStorage : ISnapshotStorage
    {
        private readonly IServiceProvider sp;
        private readonly ISerializer serializer;
        private readonly ILogger logger;
        private readonly IPostgreSqlTableStorage tableStorage;
        private readonly ConcurrentDictionary<string, string> SnapshotTableList = new ConcurrentDictionary<string, string>();

        public PostgreSqlSnapshotStorage(IServiceProvider sp, ISerializer serializer, ILogger<PostgreSqlSnapshotStorage> logger, IPostgreSqlTableStorage tableStorage)
        {
            this.sp = sp;
            this.logger = logger;
            this.serializer = serializer;
            this.tableStorage = tableStorage;
        }
        public async Task CreateTable(string snapshotName)
        {
            await this.tableStorage.CreateSnapshotTable(snapshotName);
        }
        public async Task<bool> DeleteAsync(string snapshotName, object stateId)
        {
            using (var db = sp.GetRequiredService<IPostgreSqlDbContext>())
            {
                string sql = $"DELETE FROM {snapshotName} where stateid=@StateId";
                return await db.ExecuteAsync(sql, new { StateId = stateId.ToString() }) > 0;
            }
        }
        public async Task<bool> InsertAsync<TState>(string snapshotName, object stateId, TState state) where TState : IState, new()
        {
            using (var db = sp.GetRequiredService<IPostgreSqlDbContext>())
            {
                string sql = $"INSERT into {snapshotName}(stateid,data)VALUES(@StateId,@Data)";
                var data = this.serializer.Serialize(state);
                return await db.ExecuteAsync(sql, new { StateId = stateId.ToString(), Data = data }) > 0;
            }
        }
        public async Task<TState> ReadAsync<TState>(string snapshotName, object stateId) where TState : IState, new()
        {
            using (var db = sp.GetRequiredService<IPostgreSqlDbContext>())
            {
                string sql = $"select data FROM {snapshotName} where stateid=@StateId";
                var data = await db.ExecuteScalarAsync<byte[]>(sql, new { StateId = stateId.ToString() });
                return this.serializer.Deserialize<TState>(data);
            }
        }
        public async Task<bool> UpdateAsync<TState>(string snapshotName, object stateId, TState state) where TState : IState, new()
        {
            using (var db = sp.GetRequiredService<IPostgreSqlDbContext>())
            {
                string sql = $"Update {snapshotName} set data=@Data where stateid=@StateId";
                var data = this.serializer.Serialize(state);
                return await db.ExecuteAsync(sql, new { StateId = stateId.ToString(), Data = data }) > 0;
            }
        }

   
    }
}
