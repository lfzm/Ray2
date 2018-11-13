using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using Orleans.Runtime;
using Ray2.Configuration;
using Ray2.EventSource;
using Ray2.Internal;
using Ray2.Serialization;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.PostgreSQL
{
    public class PostgreSqlEventStorage : IPostgreSqlEventStorage
    {
        private readonly DataflowBufferBlock<EventBufferWrap> dataflowBufferBlock;
        private readonly IServiceProvider _serviceProvider;
        private readonly IInternalConfiguration _internalConfiguration;
        private readonly ILogger _logger;
        private readonly PostgreSqlOptions options;
        private readonly string providerName;
        private readonly string tableName;
        private string insertSql;
        private string insertBinarySql;

        public PostgreSqlEventStorage(IServiceProvider serviceProvider, PostgreSqlOptions options, string name, string tableName)
        {
            this.providerName = name;
            this.tableName = tableName;
            this.options = options;
            this._serviceProvider = serviceProvider;
            this._logger = serviceProvider.GetRequiredService<ILogger<PostgreSqlStateStorage>>();
            this._internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            this.dataflowBufferBlock = new DataflowBufferBlock<EventBufferWrap>(this.LazySaveAsync);
            this.BuildSql(tableName);
        }
        public Task<List<EventModel>> GetListAsync(EventQueryModel query)
        {
            var list = new List<EventModel>(query.Limit);
            using (var conn = PostgreSqlDbContext.Create(this.options))
            {
                StringBuilder sql = new StringBuilder($"COPY (SELECT typecode,data,datatype,version FROM {tableName} WHERE version > '{query.StartVersion}'");
                if (query.StateId != null)
                    sql.Append($" and stateid = '{query.StateId}'");
                if (query.StartVersion > 0)
                    sql.Append($" and version <= '{query.EndVersion}'");
                if (!string.IsNullOrEmpty(query.EventTypeCode))
                    sql.Append($" and typecode = '{query.EventTypeCode}'");
                if (!string.IsNullOrEmpty(query.RelationEvent))
                    sql.Append($" and relationevent = '{query.RelationEvent}'");
                sql.Append(" ORDER BY version ASC) TO STDOUT(FORMAT BINARY)");

                using (var reader = conn.BeginBinaryExport(sql.ToString()))
                {
                    while (reader.StartRow() != -1)
                    {
                        string typeCode = reader.Read<string>(NpgsqlDbType.Varchar);
                        byte[] dataBytes = reader.Read<byte[]>(NpgsqlDbType.Bytea);
                        string dataType = reader.Read<string>(NpgsqlDbType.Varchar);
                        long version = reader.Read<Int64>(NpgsqlDbType.Bigint);

                        //Get event type
                        if (this._internalConfiguration.GetEvenType(typeCode, out Type type))
                        {
                            object data = this.GetSerializer(dataType).Deserialize(type, dataBytes);
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
        public Task<EventModel> GetAsync(object stateId, long version)
        {
            using (var db = PostgreSqlDbContext.Create(this.options))
            {
                StringBuilder sql = new StringBuilder($"COPY (SELECT typecode,data,datatype FROM {tableName} WHERE stateid = '{stateId.ToString()}' and  version = '{version}'");
                using (var reader = db.BeginBinaryExport(sql.ToString()))
                {
                    while (reader.StartRow() != -1)
                    {
                        string typeCode = reader.Read<string>(NpgsqlDbType.Varchar);
                        byte[] dataBytes = reader.Read<byte[]>(NpgsqlDbType.Bytea);
                        string dataType = reader.Read<string>(NpgsqlDbType.Varchar);

                        //Get event type
                        if (this._internalConfiguration.GetEvenType(typeCode, out Type type))
                        {
                            object data = this.GetSerializer(dataType).Deserialize(type, dataBytes);
                            EventModel eventModel = new EventModel(data, typeCode, version);
                            return Task.FromResult(eventModel);
                        }
                        else
                        {
                            this._logger.LogWarning($"{typeCode} event type not exist to IInternalConfiguration");
                        }
                    }
                    return Task.FromResult<EventModel>(null);
                }
            }
        }
        public async Task SaveAsync(List<EventBufferWrap> events)
        {
            using (var db = PostgreSqlDbContext.Create(this.options))
            {
                await db.OpenAsync();
                try
                {
                    var eventList = events.Select(f => f.Value).ToList<EventStorageModel>();
                    this.BinarySaveAsync(db, eventList);
                    events.ForEach(wrap => wrap.TaskSource.TrySetResult(true));
                    return;
                }
                catch
                {
                    foreach (var e in events)
                    {
                        await this.dataflowBufferBlock.SendAsync(e);
                    }
                }
            }
        }
        public Task<bool> SaveAsync(EventCollectionStorageModel events)
        {
            using (var db = PostgreSqlDbContext.Create(this.options))
            {
                this.BinarySaveAsync(db, events.Events);
                return Task.FromResult(true);
            }
        }
        public void BinarySaveAsync(PostgreSqlDbContext db, List<EventStorageModel> events)
        {
            using (var writer = db.BeginBinaryImport(this.insertBinarySql))
            {
                foreach (var e in events)
                {
                    var data = this.GetSerializer().Serialize(e.Event);
                    writer.StartRow();
                    writer.Write(e.StateId.ToString(), NpgsqlDbType.Varchar);
                    writer.Write(e.Event.RelationEvent, NpgsqlDbType.Varchar);
                    writer.Write(e.Event.TypeCode, NpgsqlDbType.Varchar);
                    writer.Write(data, NpgsqlDbType.Bytea);
                    writer.Write(this.options.SerializationType, NpgsqlDbType.Varchar);
                    writer.Write(e.Event.Version, NpgsqlDbType.Bigint);
                    writer.Write(e.Event.Timestamp, NpgsqlDbType.Bigint);
                }
                writer.Complete();
            }
        }
        public async Task LazySaveAsync(BufferBlock<EventBufferWrap> eventBuffer)
        {
            using (var db = PostgreSqlDbContext.Create(this.options))
            {
                while (eventBuffer.TryReceive(out var wrap))
                {
                    await this.SqlSaveAsync(db, wrap);
                }
            }
        }
        public async Task SqlSaveAsync(PostgreSqlDbContext db, EventBufferWrap wrap)
        {
            try
            {
                EventSingleStorageModel model = wrap.Value;
                var data = this.GetSerializer().Serialize(model.Event);

                wrap.Result = await db.ExecuteAsync(this.insertSql, new
                {
                    StateId = model.StateId,
                    RelationEvent = model.Event.RelationEvent,
                    TypeCode = model.Event.TypeCode,
                    Data = data,
                    DataType = this.options.SerializationType,
                    Version = model.Event.Version,
                    AddTime = model.Event.Timestamp
                }) > 0;
                wrap.TaskSource.TrySetResult(wrap.Result);
            }
            catch (Exception ex)
            {
                wrap.TaskSource.TrySetException(ex);
            }
        }
        private ISerializer GetSerializer(string name)
        {
            return this._serviceProvider.GetRequiredServiceByName<ISerializer>(name);
        }
        private ISerializer GetSerializer()
        {
            return this.GetSerializer(this.options.SerializationType);
        }
        private void BuildSql(string tableName)
        {
            this.insertSql = $"INSERT INTO {tableName}(StateId,RelationEvent,TypeCode,Data,DataType,Version,AddTime) VALUES(@StateId,@RelationEvent,@TypeCode,@Data,@DataType,@Version,@AddTime) ON CONFLICT ON CONSTRAINT {tableName}_id_unique DO NOTHING";
            this.insertBinarySql = $"COPY {tableName}(StateId,RelationEvent,TypeCode,Data,DataType,Version,AddTime) FROM STDIN (FORMAT BINARY)";
        }
    }
}
