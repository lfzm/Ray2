using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public class PostgreSqlTableStorage : IPostgreSqlTableStorage
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
            this._options = serviceProvider.GetRequiredService<IOptionsSnapshot<PostgreSqlOptions>>().Get(name);
        }

        public void CreateEventTable(string name, object stateId)
        {
            tableCache.GetOrAdd(name, (n) =>
            {
                Task task = this.CreateTable(n, stateId, CreateEventTableSql);
                task.Wait(5000);
                return n;
            });
        }

        public void CreateStateTable(string name, object stateId)
        {
            tableCache.GetOrAdd(name, (n) =>
           {
               Task task = this.CreateTable(n, stateId, CreateStateTableSql);
               task.Wait(5000);
               return n;
           });
        }

        private async Task CreateTable(string name, object stateId, string sql)
        {
            try
            {
                int stateIdLength = this.GetStateIdLength(stateId);
                sql = string.Format(sql, name, stateIdLength);
                using (var db = PostgreSqlDbContext.Create(this._options))
                {
                    await db.OpenAsync();
                    await db.ExecuteAsync(sql);
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"[{ProviderName}] Creating table {name} failed");
                throw ex;
            }
        }

        private int GetStateIdLength(object stateId)
        {
            if (stateId == null)
            {
                return 32;
            }
            else if (stateId.GetType() == typeof(int))
            {
                return 11;
            }
            else if (stateId.GetType() == typeof(long))
            {
                return 20;
            }
            else if (stateId.GetType() == typeof(string))
            {
                return 32;
            }
            else
                return 32;
        }

        private const string CreateStateTableSql = @"
                    CREATE TABLE IF NOT EXISTS {0}(
                        StateId varchar({1}) NOT NULL PRIMARY KEY,
                        DataType varchar(20) NOT NULL,  
                        Data bytea NOT NULL)";

        private const string CreateEventTableSql = @"
                    CREATE TABLE IF NOT EXISTS {0} (
                        StateId varchar({1}) NOT NULL,
                        RelationEvent varchar(250)  NULL,
                        TypeCode varchar(100)  NOT NULL,
                        DataType varchar(20) NOT NULL,
                        Data bytea NOT NULL,
                        Version int8 NOT NULL,
                        AddTime int8 NOT NULL,
                        constraint {0}_id_unique UNIQUE(StateId,TypeCode,RelationEvent)
                    ) WITH (OIDS=FALSE);
                    CREATE UNIQUE INDEX IF NOT EXISTS {0}_Event_State_Version ON {0} USING btree(StateId, Version);";
    }
}
