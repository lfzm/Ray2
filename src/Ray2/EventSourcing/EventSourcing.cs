using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray2.Configuration;
using Ray2.Storage;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.EventSourcing
{
    public class EventSourcing<TState, TStateKey> : IEventSourcing<TState, TStateKey> where TState : IState<TStateKey>, new()
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly EventSourcingOptions _options;
        private readonly ILogger _logger;
        private readonly IStorageSharding _eventStorageSharding;
        private readonly IStorageSharding _snapshotStorageSharding;
        private readonly IStorageFactory _storageFactory;

        private IEventBufferBlock _eventBufferBlock;
        private IEventStorage _eventStorage;
        private IStatusStorage _snapshotStorage;
        private TStateKey StateId;
        private string SnapshotTableName;

        public EventSourcing(EventSourcingOptions options, IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            this._options = options;
            this._logger = this._serviceProvider.GetRequiredService<ILogger<EventSourcing<TState, TStateKey>>>();
            this._storageFactory = this._serviceProvider.GetRequiredService<IStorageFactory>();
            if (!string.IsNullOrEmpty(this._options.StorageOptions.ShardingStrategy))
            {
                this._eventStorageSharding = this._serviceProvider.GetRequiredServiceByName<IStorageSharding>(this._options.StorageOptions.ShardingStrategy);
            }
            if (!string.IsNullOrEmpty(this._options.SnapshotOptions.ShardingStrategy))
            {
                this._snapshotStorageSharding = this._serviceProvider.GetRequiredServiceByName<IStorageSharding>(this._options.SnapshotOptions.ShardingStrategy);
            }
        }

        public async Task<IEventSourcing<TState, TStateKey>> Init(TStateKey stateKey)
        {
            this.StateId = stateKey;
            this.SnapshotTableName = await this.GetSnapshotTableName();
            this._eventStorage = await this.GetEventStorageProvider();
            this._snapshotStorage = await this.GetSnapshotStorageProvider();
            this._eventBufferBlock = this._serviceProvider.GetRequiredService<IEventBufferBlockFactory>().Create(this._options.StorageOptions.StorageProvider, this._options.EventSourcingName, _eventStorage);
            return this;
        }

        public async Task<bool> SaveAsync(IEvent<TStateKey> @event)
        {
            //Sharding processing
            string storageTableName = await this.GetEventTableName();
            EventSingleStorageModel storageModel = new EventSingleStorageModel(@event.StateId.ToString(), @event, this._options.EventSourcingName, storageTableName);
            return await this._eventBufferBlock.SendAsync(storageModel);
        }
        public async Task<bool> SaveAsync(IList<IEvent<TStateKey>> events)
        {
            if (events.Count == 0)
                return true;
            string storageTableName = await this.GetEventTableName();
            EventCollectionStorageModel storageModel = new EventCollectionStorageModel(this._options.EventSourcingName, storageTableName);
            foreach (var e in events)
            {
                EventStorageModel eventModel = new EventStorageModel(e.StateId.ToString(), e);
                storageModel.Events.Add(eventModel);
            }
            return await this._eventBufferBlock.SendAsync(storageModel);
        }
        public async Task<List<IEvent>> GetListAsync(EventQueryModel queryModel)
        {
            queryModel.StateId = this.StateId.ToString();
            List<string> tables = await this.GetEventTableNameList(queryModel.StartTime);
            List<IEvent> events = new List<IEvent>();
            foreach (var t in tables)
            {
                var eventModel = await _eventStorage.GetListAsync(this._options.EventSourcingName, queryModel);
                if (eventModel == null || eventModel.Count == 0)
                    return new List<IEvent>();

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
            if (this._options.SnapshotOptions.SnapshotType == SnapshotType.NoSnapshot)
                return state;

            state = await this._snapshotStorage.ReadAsync<TState>(this.SnapshotTableName, this.StateId);
            //Get current event
            List<IEvent> events = await this.GetListAsync(new EventQueryModel(state.Version));
            if (events == null || events.Count == 0)
                return state;
            state = this.TraceAsync(state, events);
            //save snapshot
            await this.SaveSnapshotAsync(state);
            return state;

        }
        public async Task ClearSnapshotAsync()
        {
            if (this._options.SnapshotOptions.SnapshotType == SnapshotType.NoSnapshot)
                return;

            await this._snapshotStorage.DeleteAsync(this.SnapshotTableName, this.StateId);
        }
        public async Task SaveSnapshotAsync(TState state)
        {
            if (this._options.SnapshotOptions.SnapshotType == SnapshotType.NoSnapshot)
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

        public TState TraceAsync(TState state, IEvent @event)
        {
            if (@event != null)
                state.Player(@event);
            return state;
        }
        public TState TraceAsync(TState state, IList<IEvent> events)
        {
            if (events == null || events.Count == 0)
                return state;
            foreach (var @event in events)
            {
                state = this.TraceAsync(state, @event);
            }
            return state;
        }

        private async Task<string> GetSnapshotTableName()
        {
            string storageTableName = string.Empty;
            if (this._snapshotStorageSharding != null)
            {
                storageTableName = await this._snapshotStorageSharding.GetTable(this.StateId);
                if (string.IsNullOrEmpty(storageTableName))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                storageTableName = "ESnapshot_" + this._options.EventSourcingName;
            }
            return storageTableName;
        }
        private async Task<string> GetEventTableName()
        {
            string storageTableName = string.Empty;
            if (this._eventStorageSharding != null)
            {
                storageTableName = await this._eventStorageSharding.GetTable(this.StateId);
                if (string.IsNullOrEmpty(storageTableName))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }

            }
            else
            {
                storageTableName = "ES_" + this._options.EventSourcingName;
            }
            return storageTableName;
        }
        private async Task<List<string>> GetEventTableNameList(long? createTime)
        {
            List<string> tables = new List<string>();
            if (this._eventStorageSharding != null)
            {
                tables = await this._eventStorageSharding.GetTableList(this.StateId, createTime);
                if (tables == null || tables.Count == 0)
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }

            }
            else
            {
                tables.Add("ES_" + this._options.EventSourcingName);
            }
            return tables;
        }
        private async Task<IStatusStorage> GetSnapshotStorageProvider()
        {
            string storageProvider = string.Empty;
            if (this._snapshotStorageSharding != null)
            {
                storageProvider = await this._snapshotStorageSharding.GetProvider(this.StateId);
                if (string.IsNullOrEmpty(storageProvider))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                storageProvider = this._options.SnapshotOptions.StorageProvider;
            }

            var sp = this._storageFactory.GetStatusStorage(storageProvider);
            return sp;
        }
        private async Task<IEventStorage> GetEventStorageProvider()
        {
            string storageProvider = string.Empty;
            if (this._eventStorageSharding != null)
            {
                 storageProvider = await this._eventStorageSharding.GetProvider(this.StateId);
                if (string.IsNullOrEmpty(storageProvider))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                storageProvider = this._options.SnapshotOptions.StorageProvider;
            }

            var sp = this._storageFactory.GetEventStorage(storageProvider);
            return sp;
        }
    }
}
