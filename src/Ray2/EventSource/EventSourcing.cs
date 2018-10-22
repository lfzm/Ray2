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
    public class EventSourcing<TState, TStateKey> : EventSourcing, IEventSourcing<TState, TStateKey> where TState : IState<TStateKey>, new()
    {
        protected readonly IStorageFactory _storageFactory;
        private TStateKey StateId;
        private IEventBufferBlock _eventBufferBlock;
        private IEventStorage _eventStorage;
        private IStateStorage _snapshotStorage;
        private StorageTableInfo SnapshotTable;

        public EventSourcing(IServiceProvider serviceProvider, EventSourceOptions options, ILogger<EventSourcing<TState, TStateKey>> logger)
            : base(serviceProvider, options, logger)
        {
            this._storageFactory = new StorageFactory(this._serviceProvider, this.Options.StorageOptions);
        }
        public async Task<IEventSourcing<TState, TStateKey>> Init(TStateKey stateKey)
        {
            this.StateId = stateKey;
            this._eventStorage = await this._storageFactory.GetEventStorage(this.Options.EventSourceName, this.StateId.ToString());
            this._eventBufferBlock = this._serviceProvider.GetRequiredService<IEventBufferBlockFactory>().Create(this.Options.StorageOptions.StorageProvider, this.Options.EventSourceName, _eventStorage);

            //Get snapshot storage information
            if (this.Options.SnapshotOptions.SnapshotType != SnapshotType.NoSnapshot)
            {
                IStorageFactory snapshotStorageFactory = new StorageFactory(this._serviceProvider, this.Options.SnapshotOptions);
                this._snapshotStorage = await snapshotStorageFactory.GetSnapshotStorage(this.Options.EventSourceName, this.StateId.ToString());
                this.SnapshotTable = await snapshotStorageFactory.GetSnapshotTable(this.Options.EventSourceName, this.StateId.ToString());
            }

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
        private async Task<string> GetEventTableName()
        {
            StorageTableInfo tableInfo = await this._storageFactory.GetEventTable(this.Options.EventSourceName, this.StateId.ToString());
            return tableInfo.Name;
        }
        public async override Task<IList<IEvent>> GetListAsync(EventQueryModel queryModel)
        {
            queryModel.StateId = this.StateId.ToString();
            List<StorageTableInfo> tables = await this._storageFactory.GetEventTableList(this.Options.EventSourceName, this.StateId.ToString(), queryModel.StartTime);
            List<IEvent> events = new List<IEvent>();
            foreach (var t in tables)
            {
                var eventModel = await _eventStorage.GetListAsync(t.Name, queryModel);
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
            if (this.Options.SnapshotOptions.SnapshotType != SnapshotType.NoSnapshot)
            {
                state = await this._snapshotStorage.ReadAsync<TState>(this.SnapshotTable.Name, this.StateId);
            }
            //Get current event
            List<IEvent<TStateKey>> events = (List<IEvent<TStateKey>>)await this.GetListAsync(new EventQueryModel(state.Version));
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

            await this._snapshotStorage.DeleteAsync(this.SnapshotTable.Name, this.StateId);
        }
        public async Task SaveSnapshotAsync(TState state)
        {
            if (this.Options.SnapshotOptions.SnapshotType == SnapshotType.NoSnapshot)
                return;
            try
            {
                if (state.Version == 1)
                    await this._snapshotStorage.InsertAsync(this.SnapshotTable.Name, state.StateId, state);
                else
                    await this._snapshotStorage.UpdateAsync(this.SnapshotTable.Name, state.StateId, state);
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
    }

    public class EventSourcing : IEventSourcing
    {
        protected readonly EventSourceOptions Options;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger _logger;

        public EventSourcing(IServiceProvider serviceProvider, EventSourceOptions options, ILogger<EventSourcing> logger)
        {
            this.Options = options;
            this._serviceProvider = serviceProvider;
            this._logger = logger;
        }

        public virtual Task ClearSnapshotAsync(string stateId)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<IList<IEvent>> GetListAsync(EventQueryModel queryModel)
        {
            IStorageFactory storageFactory = new StorageFactory(this._serviceProvider, Options.StorageOptions);
            var storages = await storageFactory.GetEventStorageList(this.Options.EventSourceName);
            if (storages == null || storages.Count == 0)
            {
                throw new Exception("没有找到对应的存储提提供商");
            }
            foreach (var storage in storages)
            {
                List<string> tables = await this._storageFactory.GetEventTableList(this.Options.EventSourceName, this.StateId.ToString(), queryModel.StartTime);
            }
        }
    }
}
