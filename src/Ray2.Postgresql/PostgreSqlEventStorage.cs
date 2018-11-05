using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using Ray2.EventSource;
using Ray2.PostgreSQL.Serialization;
using Ray2.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ray2.PostgreSQL
{
    public class PostgreSqlEventStorage : IEventStorage
    {
        private readonly ConcurrentDictionary<string, string> sqlDict = new ConcurrentDictionary<string, string>();
        private readonly IServiceProvider sp;
        private readonly ILogger logger;
        public PostgreSqlEventStorage(ILogger<PostgreSqlStatusStorage> logger, IServiceProvider sp)
        {
            this.logger = logger;
            this.sp = sp;
        }
        public Task<List<EventModel>> GetListAsync(string storageTableName, EventQueryModel query)
        {
            var list = new List<EventModel>(query.Limit);
            using (var conn = sp.GetRequiredService<IPostgreSqlDbContext>())
            {
                StringBuilder sql = new StringBuilder($"COPY(SELECT typecode,dataJson,dataBinary,version from {storageTableName} WHERE version > '{query.StartVersion}'");
                if (!string.IsNullOrEmpty(query.StateId))
                    sql.Append($" and stateid = '{query.StateId}'");
                if (query.StartVersion > 0)
                    sql.Append($" and version <= '{query.EndVersion}'");
                if (!string.IsNullOrEmpty(query.EventTypeCode))
                    sql.Append($" and typecode = '{query.EventTypeCode}'");
                if (!string.IsNullOrEmpty(query.EventTypeCode))
                    sql.Append($" and relationevent = '{query.RelationEvent}'");
                sql.Append(" order by version asc) TO STDOUT(FORMAT BINARY)");

                using (var reader = conn.BeginBinaryExport(sql.ToString()))
                {
                    using (var serializer = sp.GetRequiredService<IEventSerializer>())
                    {
                        while (reader.StartRow() != -1)
                        {
                            string typeCode = reader.Read<string>(NpgsqlDbType.Varchar);
                            var dataJson = reader.Read<string>(NpgsqlDbType.Varchar);
                            byte[] dataBytes = reader.Read<byte[]>(NpgsqlDbType.Bytea);
                            long version = reader.Read<Int64>(NpgsqlDbType.Bigint);

                            object data;
                            if (dataBytes.Length > 0)
                                data = serializer.Deserialize(typeCode, dataBytes);
                            else if (!string.IsNullOrEmpty(dataJson))
                                data = serializer.Deserialize(typeCode, dataJson);
                            else
                                throw new ArgumentNullException($"{typeCode}-{version} No event data");
                            EventModel eventModel = new EventModel(data, typeCode, version);
                            list.Add(eventModel);
                        }
                    }
                }
            }
            return Task.FromResult(list);
        }
        public async Task SaveAsync(List<EventBufferWrap> wrapList)
        {
            using (var db = sp.GetRequiredService<IPostgreSqlDbContext>())
            {
                await db.OpenAsync();
                try
                {
                    await this.BinarySaveAsync(db, wrapList);
                }
                catch
                {
                    await this.SqlSaveAsync(db, wrapList);
                }
            }
        }
        public Task BinarySaveAsync(IPostgreSqlDbContext db, List<EventBufferWrap> events)
        {
            IEventStorageModel storageModel = events.First().Value;
            string tableName = storageModel.StorageTableName;
            string sql = this.GetEventBinaryInsertSql(tableName);
            using (var writer = db.BeginBinaryImport(sql))
            {
                using (var serializer = sp.GetRequiredService<IEventSerializer>())
                {
                    foreach (var e in events)
                    {
                        //Single event store
                        if (e.Value is EventSingleStorageModel model)
                        {
                            (byte[] bytes, string json) = serializer.Serialize(model.Event);
                            writer.StartRow();
                            writer.Write(model.StateId.ToString(), NpgsqlDbType.Varchar);
                            writer.Write(model.Event.RelationEvent, NpgsqlDbType.Varchar);
                            writer.Write(model.Event.TypeCode, NpgsqlDbType.Varchar);
                            writer.Write(json, NpgsqlDbType.Varchar);
                            writer.Write(bytes, NpgsqlDbType.Bytea);
                            writer.Write(model.Event.Version, NpgsqlDbType.Bigint);
                        }
                        //Bulk event storage
                        else if (e.Value is EventCollectionStorageModel models)
                        {
                            foreach (var m in models.Events)
                            {
                                (byte[] bytes, string json) = serializer.Serialize(m.Event);
                                writer.StartRow();
                                writer.Write(m.StateId.ToString(), NpgsqlDbType.Varchar);
                                writer.Write(m.Event.RelationEvent, NpgsqlDbType.Varchar);
                                writer.Write(m.Event.TypeCode, NpgsqlDbType.Varchar);
                                writer.Write(bytes, NpgsqlDbType.Varchar);
                                writer.Write(json, NpgsqlDbType.Bytea);
                                writer.Write(m.Event.Version, NpgsqlDbType.Bigint);
                            }
                        }
                    }
                }
                writer.Complete();
            }
            events.ForEach(wrap => wrap.TaskSource.TrySetResult(true));
            return Task.CompletedTask;
        }
        public async Task SqlSaveAsync(IPostgreSqlDbContext db, List<EventBufferWrap> events)
        {
            IEventStorageModel storageModel = events.First().Value;
            string tableName = storageModel.StorageTableName;
            using (var trans = db.BeginTransaction())
            {
                using (var serializer = sp.GetRequiredService<IEventSerializer>())
                {
                    try
                    {
                        string sql = this.GetEventSqlInsertSql(tableName);
                        foreach (var e in events)
                        {
                            //Single event store
                            if (e.Value is EventSingleStorageModel model)
                            {
                                (byte[] bytes, string json) = serializer.Serialize(model.Event);
                                e.Result = await db.ExecuteAsync(sql, new
                                {
                                    model.StateId,
                                    model.Event.RelationEvent,
                                    model.Event.TypeCode,
                                    DataJson = json,
                                    DataBinary = bytes,
                                    model.Event.Version
                                }, trans) > 0;
                            }
                            //Bulk event storage
                            else if (e.Value is EventCollectionStorageModel models)
                            {
                                foreach (var m in models.Events)
                                {
                                    (byte[] bytes, string json) = serializer.Serialize(m.Event);
                                    e.Result = await db.ExecuteAsync(sql, new
                                    {
                                        m.StateId,
                                        m.Event.RelationEvent,
                                        m.Event.TypeCode,
                                        DataJson = json,
                                        DataBinary = bytes,
                                        m.Event.Version
                                    }, trans) > 0;
                                }
                            }
                        }
                        trans.Commit();
                        events.ForEach(wrap => wrap.TaskSource.TrySetResult(wrap.Result));
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        events.ForEach(wrap => wrap.TaskSource.TrySetException(ex));
                    }
                }
            }

        }
        /// <summary>
        ///  Sql mode insert time data sql
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private string GetEventSqlInsertSql(string tableName)
        {
            return sqlDict.GetOrAdd("EventSqlInsertSql_" + tableName, (key) =>
            {
                return $"INSERT INTO {tableName}(stateid,relationevent,typecode,datajson,databinary,version) VALUES(@StateId,@RelationEvent,@TypeCode,@DataJson,@DataBinary,@Version) ON CONFLICT ON CONSTRAINT {tableName}_id_unique DO NOTHING";
            });
        }
        /// <summary>
        /// Binary way to insert event data sql
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private string GetEventBinaryInsertSql(string tableName)
        {
            return sqlDict.GetOrAdd("EventBinaryInsertSql_" + tableName, (key) =>
            {
                return $"copy {tableName}(stateid,relationevent,typecode,datajson,databinary,version) FROM STDIN (FORMAT BINARY)";
            });
        }

    }
}
