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
            this.BuildSql(tableName);
        }
        public async Task<List<EventModel>> GetListAsync(EventQueryModel query)
        {
            var list = new List<EventModel>(query.Limit);
            using (var db = PostgreSqlDbContext.Create(this.options))
            {
                StringBuilder sql = new StringBuilder($"COPY (SELECT typecode,data,datatype,version FROM {tableName} WHERE version > '{query.StartVersion}'");
                if (query.StateId != null)
                    sql.Append($" and stateid = '{query.StateId}'");
                if (query.EndVersion > 0)
                    sql.Append($" and version <= '{query.EndVersion}'");
                if (!string.IsNullOrEmpty(query.EventTypeCode))
                    sql.Append($" and typecode = '{query.EventTypeCode}'");
                if (!string.IsNullOrEmpty(query.RelationEvent))
                    sql.Append($" and relationevent = '{query.RelationEvent}'");
                sql.Append(" ORDER BY version ASC) TO STDOUT(FORMAT BINARY)");

                await db.OpenAsync();
                using (var reader = db.BeginBinaryExport(sql.ToString()))
                {
                    while (reader.StartRow() != -1)
                    {
                        string typeCode = reader.Read<string>(NpgsqlDbType.Varchar);
                        byte[] dataBytes = reader.Read<byte[]>(NpgsqlDbType.Bytea);
                        string dataType = reader.Read<string>(NpgsqlDbType.Varchar);
                        long version = reader.Read<Int64>(NpgsqlDbType.Bigint);

                        //Get event type
                        EventModel eventModel = this.ConversionEvent(typeCode, dataType, dataBytes, version);
                        if (eventModel != null)
                        {
                            list.Add(eventModel);
                        }
                    }
                }
            }
            return list;
        }
        public async Task<EventModel> GetAsync(object stateId, long version)
        {
            using (var db = PostgreSqlDbContext.Create(this.options))
            {
                StringBuilder sql = new StringBuilder($"COPY (SELECT typecode,data,datatype FROM {tableName} WHERE stateid = '{stateId.ToString()}' and  version = '{version}' ) TO STDOUT(FORMAT BINARY)");
                await db.OpenAsync();
                using (var reader = db.BeginBinaryExport(sql.ToString()))
                {
                    while (reader.StartRow() != -1)
                    {
                        string typeCode = reader.Read<string>(NpgsqlDbType.Varchar);
                        byte[] dataBytes = reader.Read<byte[]>(NpgsqlDbType.Bytea);
                        string dataType = reader.Read<string>(NpgsqlDbType.Varchar);

                        return this.ConversionEvent(typeCode, dataType, dataBytes, version);
                    }
                    return null;
                }
            }
        }

        public EventModel ConversionEvent(string typeCode, string dataType, byte[] bytes, long version)
        {
            //Get event type
            if (this._internalConfiguration.GetEvenType(typeCode, out Type type))
            {
                object data = this.GetSerializer(dataType).Deserialize(type, bytes);
                if (data is IEvent e)
                {
                    EventModel eventModel = new EventModel(e, typeCode, version);
                    return eventModel;
                }
                else
                {
                    this._logger.LogWarning($"{dataType}.{version}  not equal to IEvent");
                    return null;
                }
            }
            else
            {
                this._logger.LogWarning($"{typeCode} event type not exist to IInternalConfiguration");
                return null;
            }
        }
        public async Task SaveAsync(List<IDataflowBufferWrap<EventStorageModel>> eventWraps)
        {
            using (var db = PostgreSqlDbContext.Create(this.options))
            {
                await db.OpenAsync();
                try
                {
                    var eventList = eventWraps.Select(f => f.Data).ToList<EventModel>();
                    this.BinarySaveAsync(db, eventList);
                    eventWraps.ForEach(wrap => wrap.CompleteHandler(true));
                    return;
                }
                catch
                {
                    await this.SqlSaveAsync(db, eventWraps);
                }
            }
        }
        public async  Task<bool> SaveAsync(EventCollectionStorageModel events)
        {
            using (var db = PostgreSqlDbContext.Create(this.options))
            {
                await db.OpenAsync();
                this.BinarySaveAsync(db, events.Events);
                return true;
            }
        }
        public void BinarySaveAsync(PostgreSqlDbContext db, List<EventModel> events)
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

        public async Task SqlSaveAsync(PostgreSqlDbContext db, List<IDataflowBufferWrap<EventStorageModel>> eventWraps)
        {
            using (var trans = db.BeginTransaction())
            {
                try
                {

                    foreach (var wrap in eventWraps)
                    {
                        EventStorageModel model = wrap.Data;
                        var data = this.GetSerializer().Serialize(model.Event);

                        wrap.Data.Result = await db.ExecuteAsync(this.insertSql, new
                        {
                            StateId = model.StateId,
                            RelationEvent = model.Event.RelationEvent,
                            TypeCode = model.Event.TypeCode,
                            Data = data,
                            DataType = this.options.SerializationType,
                            Version = model.Event.Version,
                            AddTime = model.Event.Timestamp
                        }) > 0;
                    }
                    trans.Commit();
                    eventWraps.ForEach(wrap => wrap.CompleteHandler(wrap.Data.Result));
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    eventWraps.ForEach(wrap => wrap.ExceptionHandler(ex));
                }
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
