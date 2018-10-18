using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray2.Configuration;
using Ray2.Storage;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.EventSource
{
    public class EventSourcing<TState, TStateKey> : IEventSourcing<TState, TStateKey> where TState : IState<TStateKey>, new()
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IStorageFactory _storageFactory;
        private readonly EventSourcingOptions Options;

        private IStorageSharding _eventStorageSharding;
        private IStorageSharding _snapshotStorageSharding;
        private IEventBufferBlock _eventBufferBlock;
        private IEventStorage _eventStorage;
        private IStatusStorage _snapshotStorage;
        private TStateKey StateId;
        private string SnapshotTableName;

        public EventSourcing(EventSourcingOptions options, IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            this.Options = options;
            this._logger = this._serviceProvider.GetRequiredService<ILogger<EventSourcing<TState, TStateKey>>>();
            this._storageFactory = this._serviceProvider.GetRequiredService<IStorageFactory>();
        }
        public async Task<IEventSourcing<TState, TStateKey>> Init(TStateKey stateKey)
        {
            this.StateId = stateKey;
            this.SnapshotTableName = await this.GetSnapshotTableName();
            this._eventStorage = await this.GetEventStorageProvider();
            this._snapshotStorage = await this.GetSnapshotStorageProvider();
            this._eventBufferBlock = this._serviceProvider.GetRequiredService<IEventBufferBlockFactory>().Create(this.Options.StorageOptions.StorageProvider, this.Options.EventSourceName, _eventStorage);
            return this;
        }
        public async Task<bool> SaveAsync(IEvent<TStateKey> @event)
        {
            //Sharding processing
            string storageTableName = await this.GetEventTableName();
            EventSingleStorageModel storageModel = new EventSingleStorageModel(@event.StateId.ToString(), @event, this.Options.EventSourceName, storageTableName);
            return await this._eventBufferBlock.SendAsync(storageModel);
        }
        public async Task<bool> SaveAsync(IList<IEvent<TStateKey>> events)
        {
            if (events.Count == 0)
                return true;
            string storageTableName = await this.GetEventTableName();
            EventCollectionStorageModel storageModel = new EventCollectionStorageModel(this.Options.EventSourceName, storageTableName);
            foreach (var e in events)
            {
                EventStorageModel eventModel = new EventStorageModel(e.StateId.ToString(), e);
                storageModel.Events.Add(eventModel);
            }
            return await this._eventBufferBlock.SendAsync(storageModel);
        }
        public async Task<List<IEvent<TStateKey>>> GetListAsync(EventQueryModel queryModel)
        {
            queryModel.StateId = this.StateId.ToString();
            List<string> tables = await this.GetEventTableNameList(queryModel.StartTime);
            List<IEvent<TStateKey>> events = new List<IEvent<TStateKey>>();
            foreach (var t in tables)
            {
                var eventModel = await _eventStorage.GetListAsync(this.Options.EventSourceName, queryModel);
                if (eventModel == null || eventModel.Count == 0)
                    return new List<IEvent<TStateKey>>();

                foreach (var model in eventModel)
                {
                    if (model.Data is IEvent<TStateKey> @event)
                    {
                        events.Add(@event);
                    }
                    else
                    {
                        this._logger.LogWarning($"{model.TypeCode}.{model.Version}  not equal to {typeof(IEvent<TStateKey>).Name}");
                        continue;
                    }
                }
                if (queryModel.Limit > 0)
                {
                    if (events.Count >= queryModel.Limit)
                        break;
                }
            }
            return events;
        }

        public async Task<TState> ReadSnapshotAsync()
        {
            var state = new TState { StateId = this.StateId };
            if (this.Options.SnapshotOptions.SnapshotType == SnapshotType.NoSnapshot)
                return state;

            state = await this._snapshotStorage.ReadAsync<TState>(this.SnapshotTableName, this.StateId);
            //Get current event
            List<IEvent<TStateKey>> events = await this.GetListAsync(new EventQueryModel(state.Version));
            if (events == null || events.Count == 0)
                return state;
            state = this.TraceAsync(state, events);
            //save snapshot
            await this.SaveSnapshotAsync(state);
            return state;

        }
        public async Task ClearSnapshotAsync()
        {
            if (this.Options.SnapshotOptions.SnapshotType == SnapshotType.NoSnapshot)
                return;

            await this._snapshotStorage.DeleteAsync(this.SnapshotTableName, this.StateId);
        }
        public async Task SaveSnapshotAsync(TState state)
        {
            if (this.Options.SnapshotOptions.SnapshotType == SnapshotType.NoSnapshot)
                return;
            try
            {
                if (state.Version == 1)
                    await this._snapshotStorage.InsertAsync(this.SnapshotTableName, state.StateId, state);
                else
                    await this._snapshotStorage.UpdateAsync(this.SnapshotTableName, state.StateId, state);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"SaveSnapshotAsync {nameof(TState)}:StateId= {state.StateId} failure ;");
                return;
            }
        }

        public TState TraceAsync(TState state, IEvent<TStateKey> @event)
        {
            if (@event != null)
                state.Player(@event);
            return state;
        }
        public TState TraceAsync(TState state, IList<IEvent<TStateKey>> events)
        {
            if (events == null || events.Count == 0)
                return state;
            foreach (var @event in events)
            {
                state = this.TraceAsync(state, @event);
            }
            return state;
        }

        /// <summary>
        /// 获取事件源快照存储表名，调用分表策略进行分表。
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetSnapshotTableName()
        {
            if (!string.IsNullOrEmpty(this.SnapshotTableName))
                return this.SnapshotTableName;

            string storageTableName = string.Empty;
            if (this._snapshotStorageSharding != null)
            {
                storageTableName = await this._snapshotStorageSharding.GetTable(this.Options.EventSourceName, StorageType.EventSourceSnapshot, this.StateId);
                if (string.IsNullOrEmpty(storageTableName))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                storageTableName = this.Options.EventSourceName;
            }
            this.SnapshotTableName = StorageTableNameBuild.BuildSnapshotTableName(storageTableName);
            return this.SnapshotTableName;
        }
        /// <summary>
        /// 获取事件源存储表名，调用分表策略进行分表。
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetEventTableName()
        {
            string storageTableName = string.Empty;
            if (this._eventStorageSharding != null)
            {
                storageTableName = await this._eventStorageSharding.GetTable(this.Options.EventSourceName,  StorageType.EventSource,this.StateId);
                if (string.IsNullOrEmpty(storageTableName))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                storageTableName = this.Options.EventSourceName;
            }
            return StorageTableNameBuild.BuildEventTableName(storageTableName);
        }
        /// <summary>
        /// 获取事件源所有存储表名，调用分表策略进行分表。
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> GetEventTableNameList(long? createTime)
        {
            List<string> tables = new List<string>();
            if (this._eventStorageSharding != null)
            {
                tables = await this._eventStorageSharding.GetTableList(this.Options.EventSourceName, StorageType.EventSource, this.StateId, createTime);
                if (tables == null || tables.Count == 0)
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                tables.Add(this.Options.EventSourceName);
            }
            return tables.Select(f => StorageTableNameBuild.BuildEventTableName(f)).ToList();
        }
        /// <summary>
        /// Get a event sourcing  snapshot storage provider
        /// </summary>
        /// <returns></returns>
        private async Task<IStatusStorage> GetSnapshotStorageProvider()
        {
            string storageProvider = string.Empty;
            if (!string.IsNullOrEmpty(this.Options.SnapshotOptions.ShardingStrategy))
            {
                this._snapshotStorageSharding = this._serviceProvider.GetRequiredServiceByName<IStorageSharding>(this.Options.SnapshotOptions.ShardingStrategy);
                storageProvider = await this._snapshotStorageSharding.GetProvider(this.Options.EventSourceName, StorageType.EventSourceSnapshot, this.StateId);
                if (string.IsNullOrEmpty(storageProvider))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                storageProvider = this.Options.SnapshotOptions.StorageProvider;
            }

            var sp = this._storageFactory.GetStatusStorage(storageProvider);
            return sp;
        }
        /// <summary>
        /// Get a event sourcing  storage provider
        /// </summary>
        /// <returns></returns>
        private async Task<IEventStorage> GetEventStorageProvider()
        {
            string storageProvider = string.Empty;
            if (!string.IsNullOrEmpty(this.Options.StorageOptions.ShardingStrategy))
            {
                this._eventStorageSharding = this._serviceProvider.GetRequiredServiceByName<IStorageSharding>(this.Options.StorageOptions.ShardingStrategy);
                storageProvider = await this._eventStorageSharding.GetProvider(this.Options.EventSourceName, StorageType.EventSource, this.StateId);
                if (string.IsNullOrEmpty(storageProvider))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                storageProvider = this.Options.SnapshotOptions.StorageProvider;
            }
            var sp = this._storageFactory.GetEventStorage(storageProvider);
            return sp;
        }
    }
}
