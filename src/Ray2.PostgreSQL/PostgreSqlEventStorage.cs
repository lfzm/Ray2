using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using Ray2.Configuration;
using Ray2.EventSource;
using Ray2.PostgreSQL.Configuration;
using Ray2.Serialization;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly ISerializer _serializer;
        private readonly IInternalConfiguration _internalConfiguration;
        private readonly ILogger _logger;
        private readonly PostgreSqlOptions _options;

        public PostgreSqlEventStorage(IServiceProvider serviceProvider, PostgreSqlOptions options)
        {
            this._serviceProvider = serviceProvider;
            this._logger = serviceProvider.GetRequiredService<ILogger<PostgreSqlStatusStorage>>();
            this._serializer = serviceProvider.GetRequiredService<ISerializer>();
            this._internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            this._options = options;
        }
        public Task<List<EventModel>> GetListAsync(string storageTableName, EventQueryModel query)
        {
            var list = new List<EventModel>(query.Limit);
            using (var conn = this.GetDbContext())
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
                    while (reader.StartRow() != -1)
                    {
                        string typeCode = reader.Read<string>(NpgsqlDbType.Varchar);
                        var dataJson = reader.Read<string>(NpgsqlDbType.Varchar);
                        byte[] dataBytes = reader.Read<byte[]>(NpgsqlDbType.Bytea);
                        long version = reader.Read<Int64>(NpgsqlDbType.Bigint);

                        //Get event type
                        if (this._internalConfiguration.GetEvenType(typeCode, out Type type))
                        {
                            object data;
                            if (dataBytes.Length > 0)
                                data = _serializer.Deserialize(type, dataBytes);
                            else if (!string.IsNullOrEmpty(dataJson))
                                data = _serializer.Deserialize(type, dataJson);
                            else
                                throw new ArgumentNullException($"{typeCode}-{version} No event data");

                            EventModel eventModel = new EventModel(data, typeCode, version);
                            list.Add(eventModel);
                        }
                        else
                        {
                            this._logger.LogWarning($"{typeCode} event type not exist to IInternalConfiguration");
                        }
                    }

                }
            }
            return Task.FromResult(list);
        }
        public async Task SaveAsync(List<EventBufferWrap> wrapList)
        {
            using (var db = this.GetDbContext())
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
        public Task BinarySaveAsync(PostgreSqlDbContext db, List<EventBufferWrap> events)
        {
            IEventStorageModel storageModel = events.First().Value;
            string tableName = storageModel.StorageTableName;
            string sql = this.GetEventBinaryInsertSql(tableName);
            using (var writer = db.BeginBinaryImport(sql))
            {
                foreach (var e in events)
                {
                    //Single event store
                    if (e.Value is EventSingleStorageModel model)
                    {
                        (byte[] bytes, string json) = this.Serialize(model.Event);
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
                            (byte[] bytes, string json) = this.Serialize(m.Event);
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
                writer.Complete();
            }
            events.ForEach(wrap => wrap.TaskSource.TrySetResult(true));
            return Task.CompletedTask;
        }
        public async Task SqlSaveAsync(PostgreSqlDbContext db, List<EventBufferWrap> events)
        {
            IEventStorageModel storageModel = events.First().Value;
            string tableName = storageModel.StorageTableName;
            using (var trans = db.BeginTransaction())
            {
                try
                {
                    string sql = this.GetEventSqlInsertSql(tableName);
                    foreach (var e in events)
                    {
                        //Single event store
                        if (e.Value is EventSingleStorageModel model)
                        {
                            (byte[] bytes, string json) = this.Serialize(model.Event);
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
                                (byte[] bytes, string json) = this.Serialize(m.Event);
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

        private PostgreSqlDbContext GetDbContext()
        {
            return new PostgreSqlDbContext(this._options);
        }

        private (byte[], string) Serialize(IEvent @event)
        {
            string str = String.Empty;
            byte[] bytes = new byte[0];
            if (this._options.SerializationType == SerializationType.Byte)
            {
                bytes = this._serializer.Serialize(@event);
            }
            else
            {
                str = this._serializer.SerializeString(@event);
            }

            return ValueTuple.Create<byte[], string>(new byte[0], str);
        }
    }
}
