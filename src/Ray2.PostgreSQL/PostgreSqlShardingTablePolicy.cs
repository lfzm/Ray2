using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ray2.PostgreSQL.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Ray2.PostgreSQL
{
    public class PostgreSqlShardingTablePolicy : IShardingTablePolicy
    {
        private readonly PostgreSqlOptions options;
        private readonly ILogger logger;
        private readonly IPostgreSqlTableStorage tableStorage;

        private ConcurrentDictionary<string, ShardingTableStatus> ShardTableStatusList = new ConcurrentDictionary<string, ShardingTableStatus>();
        private ConcurrentDictionary<string, List<TableInfo>> ShardTableInfoList = new ConcurrentDictionary<string, List<TableInfo>>();
        public PostgreSqlShardingTablePolicy(PostgreSqlOptions options, ILogger<PostgreSqlShardingTablePolicy> logger, IPostgreSqlTableStorage shardingTableStorage)
        {
            this.options = options;
            this.logger = logger;
            this.tableStorage = shardingTableStorage;
        }

        public Task<List<string>> GetShardingTableName(string eventSourceName, long? startTime)
        {
            var tables = ShardTableInfoList.GetOrAdd(eventSourceName, (key) =>
            {
                var result = this.tableStorage.GetEventTableList(eventSourceName);
                result.Wait();
                return result.Result;
            });
            if (startTime != null)
            {
                tables = tables.Where(f => f.CreateTime >= startTime).ToList();
            }
            var list = tables.Select(f => f.Name).ToList();
            return Task.FromResult(list);
        }

        public async Task<string> GetShardingTableName(IEventStorageModel storageModel)
        {
            if (!storageModel.IsTableSharding)
                return storageModel.EventSourceName;

            return "";
        }
        public void TotalEventStorageCount(string eventSourceName, int count)
        {
            
        }
    }


}
