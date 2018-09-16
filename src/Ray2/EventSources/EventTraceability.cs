using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ray2.Configuration;
using Ray2.Exceptions;
using Ray2.Storage;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.EventSources
{
    public class EventTraceability<TState, TStateKey> : IEventTraceability<TState, TStateKey> where TState : IState<TStateKey>, new()
    {
        private readonly ILogger logger;
        private readonly IStorageFactory storageFactory;
        private readonly IServiceProvider serviceProvider;
        private IStorageSharding storageSharding;
        private StateSnapshotConfig config;
        private ISnapshotStorage snapshotStorage;
        private TStateKey stateKey;
        public EventTraceability(IStorageFactory storageFactory,
            IServiceProvider serviceProvider,
            ILogger<EventTraceability<TState, TStateKey>> logger)
        {
            this.serviceProvider = serviceProvider;
            this.storageFactory = storageFactory;
            this.logger = logger;
        }
        public void Injection(TStateKey stateKey, StateSnapshotConfig config)
        {
            this.stateKey = stateKey;
            this.config = config;
            this.storageSharding = this.serviceProvider.GetRequiredServiceByName<IStorageSharding>("ET_" + this.config.SnapshotName);
            string storageProviderName = this.storageSharding.GetProvider(stateKey);
            if (string.IsNullOrEmpty(storageProviderName))
            {
                throw new ArgumentNullException("IStorageSharding failed to get storage provider");
            }
            this.snapshotStorage = storageFactory.GetSnapshotStorage(storageProviderName);
        }

        public async Task<TState> ReadSnapshotAsync()
        {
            var state = new TState { StateId = stateKey };
            if (config.SnapshotType == SnapshotType.NoSnapshot)
                return state;

            return await this.snapshotStorage.ReadAsync<TState>(this.config.SnapshotName, stateKey);

        }
        public Task ClearSnapshotAsync()
        {
            if (config.SnapshotType == SnapshotType.NoSnapshot)
                return Task.CompletedTask;
            return this.snapshotStorage.DeleteAsync(this.config.SnapshotName, stateKey);
        }
        public Task SaveSnapshotAsync(TState state)
        {
            if (config.SnapshotType == SnapshotType.NoSnapshot)
                return Task.CompletedTask;
            try
            {
                if (state.Version == 1)
                    return this.snapshotStorage.InsertAsync(this.config.SnapshotName, state.StateId, state);
                else
                    return this.snapshotStorage.UpdateAsync(this.config.SnapshotName, state.StateId, state);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"SaveSnapshotAsync {nameof(TState)}:StateId= {state.StateId} failure ;");
                return Task.CompletedTask;
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
        public async Task<TState> TraceAsync(IEventSourcing<TState, TStateKey> eventSourcing)
        {
            if (eventSourcing == null)
                throw new ArgumentNullException("The corresponding event source cannot be empty");
            //Get a snapshot first
            TState state = await this.ReadSnapshotAsync();
            //Get current event
            List<IEvent<TStateKey>> events = await eventSourcing.GetListAsync(new EventQueryModel(state.Version));
            if (events == null || events.Count == 0)
                return state;
            state = this.TraceAsync(state, events);
            //save snapshot
            await this.SaveSnapshotAsync(state);
            return state;
        }
      
    }
}
