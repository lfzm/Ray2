using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ray2.PostgreSQL
{
    internal class PostgreSqlTableStorage : IPostgreSqlTableStorage
    {
        private readonly IServiceProvider sp;
        private readonly ILogger logger;
        public PostgreSqlTableStorage(IServiceProvider sp, ILogger<PostgreSqlTableStorage> logger)
        {
            this.sp = sp;
            this.logger = logger;
        }
        public async Task CreateEventTable(TableInfo table)
        {
            using (var db = sp.GetRequiredService<IPostgreSqlDbContext>())
            {
                using (var trans = db.BeginTransaction())
                {
                    try
                    {
                        await db.ExecuteAsync(string.Format(CreateEventTableSql, table.Name, 50), transaction: trans);
                        await db.ExecuteAsync(InsertShardingTableSql, table, trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, $"Creating event table {table.Name} failed");
                        trans.Rollback();
                        throw ex;
                    }
                }
            }
        }
        public async Task CreateShardingTable()
        {
            try
            {
                using (var db = sp.GetRequiredService<IPostgreSqlDbContext>())
                {
                    await db.ExecuteAsync(CreateShardingTableSql);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Initialization failed to create ray_ShardingTableList table");
                throw ex;
            }
        }
        public async Task CreateSnapshotTable(string name)
        {
            try
            {
                using (var db = sp.GetRequiredService<IPostgreSqlDbContext>())
                {
                    string sql = string.Format(CreateSnapshotTableSql, name, 50);
                    await db.ExecuteAsync(sql);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to create snapshot table {name}");
                throw ex;
            }
        }
        public async Task<List<TableInfo>> GetEventTableList(string prefix)
        {
            //If the query sub-table information table does not exist, create it.
            using (var db = sp.GetRequiredService<IPostgreSqlDbContext>())
            {
                string sql = " SELECT (to_regclass('ray_shardingtablelist') is not null) As exists ";
                string result = await db.ExecuteScalarAsync<string>(sql);
                if (result.Equals("f", StringComparison.OrdinalIgnoreCase))
                {
                    await this.CreateShardingTable();
                }
                sql = "SELECT * FROM ray_shardingtablelist where prefix=@Prefix order by version asc";
                return (await db.QueryAsync<TableInfo>(sql, new { Prefix = prefix })).AsList();
            }
        }
        public async Task<long> GetEventTableCount(string name)
        {
            using (var db = sp.GetRequiredService<IPostgreSqlDbContext>())
            {
                string sql = $"SELECT count(*) FROM {name} ";
                long count = await db.ExecuteScalarAsync<long>(sql);
                return count;
            }
        }

        private const string CreateShardingTableSql = @"
                    CREATE TABLE IF Not EXISTS ray_shardingtablelist(
                        Prefix varchar(255) not null,
                        Name varchar(255) not null,
                        Version int4,
                        CreateTime bigint
                    )WITH (OIDS=FALSE);
                    CREATE UNIQUE INDEX IF NOT EXISTS table_version ON ray_shardingtablelist USING btree(Prefix, Version)";

        private const string CreateSnapshotTableSql = @"
                    CREATE TABLE if not exists {0}(
                        StateId varchar({1}) not null PRIMARY KEY,
                        Data bytea not null)";

        private const string CreateEventTableSql = @"
                    CREATE TABLE {0} (
                        StateId varchar({1}) not null,
                        RelationEvent varchar(250)  null,
                        TypeCode varchar(100)  not null,
                        DataJson text null,
                        DataBinary bytea null,
                        Version int8 not null,
                        constraint {0}_id_unique unique(StateId,TypeCode,RelationEvent)
                    ) WITH (OIDS=FALSE);
                    CREATE UNIQUE INDEX {0}_Event_State_Version ON {0} USING btree(StateId, Version);";

        private const string InsertShardingTableSql = @"
                    INSERT into ray_shardingtablelist VALUES(@Prefix,@Name,@Version,@CreateTime)";


    }
}
