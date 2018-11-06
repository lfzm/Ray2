using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ray2.Configuration;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.EventSource
{
    public class EventSourcing<TState, TStateKey> : EventSourcing, IEventSourcing<TState, TStateKey> where TState : IState<TStateKey>, new()
    {
        private IStorageFactory _storageFactory;
        private TStateKey StateId;
        private IEventBufferBlock _eventBufferBlock;
        private IEventStorage _eventStorage;
        private IStateStorage _snapshotStorage;
        private string SnapshotTable;

        public EventSourcing(IServiceProvider serviceProvider, ILogger<EventSourcing<TState, TStateKey>> logger)
            : base(serviceProvider, logger)
        {

        }
        public async Task<IEventSourcing<TState, TStateKey>> Init(TStateKey stateKey)
        {
            this.StateId = stateKey;
            this._storageFactory = new StorageFactory(this._serviceProvider, this.Options.StorageOptions);
            this._eventStorage = await this._storageFactory.GetEventStorage(this.Options.EventSourceName, this.StateId.ToString());
            this._eventBufferBlock = this._serviceProvider.GetRequiredService<IEventBufferBlockFactory>().Create(this.Options.StorageOptions.StorageProvider, this.Options.EventSourceName, _eventStorage);

            //Get snapshot storage information
            if (this.Options.SnapshotOptions.SnapshotType != SnapshotType.NoSnapshot)
            {
                IStorageFactory snapshotStorageFactory = new StorageFactory(this._serviceProvider, this.Options.SnapshotOptions);
                this._snapshotStorage = await snapshotStorageFactory.GetStateStorage(this.Options.EventSourceName, StorageType.EventSourceSnapshot, this.StateId.ToString());
                this.SnapshotTable = await snapshotStorageFactory.GetTable(this.Options.EventSourceName, StorageType.EventSourceSnapshot, this.StateId.ToString());
            }
            return this;
        }
        public async Task<bool> SaveAsync(IEvent<TStateKey> @event)
        {
            //Sharding processing
            string storageTableName = await this.GetEventTableName();
            EventSingleStorageModel storageModel = new EventSingleStorageModel(@event.StateId, @event, this.Options.EventSourceName, storageTableName);
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
                EventStorageModel eventModel = new EventStorageModel(e.StateId, e);
                storageModel.Events.Add(eventModel);
            }
            return await this._eventBufferBlock.SendAsync(storageModel);
        }
        private async Task<string> GetEventTableName()
        {
            return await this._storageFactory.GetTable(this.Options.EventSourceName, StorageType.EventSource, this.StateId.ToString());
        }
        public async override Task<IList<IEvent>> GetListAsync(EventQueryModel queryModel)
        {
            queryModel.StateId = this.StateId.ToString();
            List<string> tables = await this._storageFactory.GetTableList(this.Options.EventSourceName, StorageType.EventSource, this.StateId.ToString(), queryModel.StartTime);
            List<IEvent> events = new List<IEvent>();
            foreach (var t in tables)
            {
                var eventModels = await _eventStorage.GetListAsync(t, queryModel);
                var _events = this.ConvertEvent<TStateKey>(eventModels);
                events.AddRange(_events);
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
                state = await this._snapshotStorage.ReadAsync<TState>(this.SnapshotTable, this.StateId);
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

            await this._snapshotStorage.DeleteAsync(this.SnapshotTable, this.StateId);
        }
        public async Task SaveSnapshotAsync(TState state)
        {
            if (this.Options.SnapshotOptions.SnapshotType == SnapshotType.NoSnapshot)
                return;
            try
            {
                if (state.Version == 1)
                    await this._snapshotStorage.InsertAsync(this.SnapshotTable, state.StateId, state);
                else
                    await this._snapshotStorage.UpdateAsync(this.SnapshotTable, state.StateId, state);
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
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger _logger;

        public EventSourcing(IServiceProvider serviceProvider, ILogger<EventSourcing> logger)
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger;
        }
        public EventSourceOptions Options { get;  set; }

        public virtual async Task ClearSnapshotAsync(string stateId)
        {
            IStorageFactory storageFactory = new StorageFactory(this._serviceProvider, Options.SnapshotOptions);
            var storageTable = await storageFactory.GetTable(this.Options.EventSourceName, StorageType.EventSourceSnapshot, stateId);
            var storage = await storageFactory.GetStateStorage(this.Options.EventSourceName, StorageType.EventSourceSnapshot, stateId);
            await storage.DeleteAsync(storageTable, stateId);
        }

        public virtual async Task<IList<IEvent>> GetListAsync(EventQueryModel queryModel)
        {
            IStorageFactory storageFactory = new StorageFactory(this._serviceProvider, Options.StorageOptions);
            var storages = await storageFactory.GetEventStorageList(this.Options.EventSourceName, queryModel.StartTime);
            List<IEvent> events = new List<IEvent>();
            foreach (var storage in storages)
            {
                foreach (var table in storage.Tables)
                {
                    var eventModels = await storage.Storage.GetListAsync(table, queryModel);
                    var _events = this.ConvertEvent(eventModels);
                    events.AddRange(_events);
                    if (queryModel.Limit > 0)
                    {
                        if (events.Count >= queryModel.Limit)
                            break;
                    }
                }
            }
            return events;
        }

        protected List<IEvent> ConvertEvent(IList<EventModel> eventModels)
        {
            List<IEvent> events = new List<IEvent>();
            if (eventModels == null || eventModels.Count == 0)
                return new List<IEvent>();

            foreach (var model in eventModels)
            {
                if (model.Data is IEvent @event)
                {
                    events.Add(@event);
                }
                else
                {
                    this._logger.LogWarning($"{model.TypeCode}.{model.Version}  not equal to IEvent");
                    continue;
                }
            }
            return events;
        }
        protected List<IEvent> ConvertEvent<TStateKey>(IList<EventModel> eventModels)
        {
            List<IEvent> events = new List<IEvent>();
            if (eventModels == null || eventModels.Count == 0)
                return new List<IEvent>();

            foreach (var model in eventModels)
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
            return events;
        }
    }
}
