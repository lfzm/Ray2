using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ray2.PostgreSQL.Configuration;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    internal class PostgreSqlTableStorage : IPostgreSqlTableStorage
    {
        private readonly ConcurrentDictionary<string, string> tableCache = new ConcurrentDictionary<string, string>();
        private readonly IServiceProvider _serviceProvider;
        private readonly PostgreSqlOptions _options;
        private readonly ILogger _logger;
        private readonly string ProviderName;
        public PostgreSqlTableStorage(IServiceProvider serviceProvider, string name)
        {
            this.ProviderName = name;
            this._serviceProvider = serviceProvider;
            this._logger = serviceProvider.GetRequiredService<ILogger<PostgreSqlTableStorage>>();
            this._options = serviceProvider.GetRequiredService<OptionsManager<PostgreSqlOptions>>().Get(name);
        }
        public Task CreateEventTable(string name)
        {
            tableCache.GetOrAdd(name, (n) =>
            {
                this.CreateTable(CreateEventTableSql).GetAwaiter().GetResult();
                return n;
            });
            return Task.CompletedTask;
        }

        public Task CreateStateTable(string name)
        {
            tableCache.GetOrAdd(name, (n) =>
            {
                this.CreateTable(CreateStateTableSql).GetAwaiter().GetResult();
                return n;
            });
            return Task.CompletedTask;
        }

        private async Task CreateTable(string sql)
        {
            try
            {
                using (var db = PostgreSqlDbContext.Create(this._options))
                {
                    await db.ExecuteAsync(sql);
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Initialization failed to create ray_ShardingTableList table");
                throw ex;
            }
        }

        private const string CreateStateTableSql = @"
                    CREATE TABLE IF NOT EXISTS {0}(
                        StateId varchar({1}) NOT NULL PRIMARY KEY,
                        Data bytea NOT NULL)";

        private const string CreateEventTableSql = @"
                    CREATE TABLE IF NOT EXISTS {0} (
                        StateId varchar({1}) NOT NULL,
                        RelationEvent varchar(250)  NULL,
                        TypeCode varchar(100)  NOT NULL,
                        DataJson text NULL,
                        DataBinary bytea NULL,
                        Version int8 NOT NULL,
                        constraint {0}_id_unique UNIQUE(StateId,TypeCode,RelationEvent)
                    ) WITH (OIDS=FALSE);
                    CREATE UNIQUE INDEX {0}_Event_State_Version ON {0} USING btree(StateId, Version);";
    }
}
