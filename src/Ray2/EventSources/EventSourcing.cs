using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using Ray2.Configuration;
using Ray2.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ray2.EventSources
{
    public class EventSourcing<TState, TStateKey> : IEventSourcing<TState, TStateKey> where TState : IState<TStateKey>, new()
    {
        private readonly ILogger logger;
        private readonly IEventBufferBlockFactory eventBufferBlockFactory;
        private readonly IStorageFactory storageFactory;
        private readonly IServiceProvider serviceProvider;
        private IStorageSharding storageSharding;
        private IEventBufferBlock eventBufferBlock;
        private EventSourcesConfig config;
        private IEventStorage eventStorage;
        private TStateKey stateKey;
        public EventSourcing(ILogger logger, IEventBufferBlockFactory eventBufferBlockFactory, IStorageFactory storageFactory, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.eventBufferBlockFactory = eventBufferBlockFactory;
            this.storageFactory = storageFactory;
            this.serviceProvider = serviceProvider;
        }
        public void Injection(TStateKey stateKey, EventSourcesConfig config)
        {
            //Get the storage provider name according to the sharding database policy
            this.config = config;
            this.stateKey = stateKey;
            this.storageSharding = this.serviceProvider.GetRequiredServiceByName<IStorageSharding>("ES_" + config.EventSourceName);
            string storageProviderName = this.storageSharding.GetProvider(stateKey);
            if (string.IsNullOrEmpty(storageProviderName))
            {
                throw new ArgumentNullException("IStorageSharding failed to get storage provider");
            }
            this.eventStorage = storageFactory.GetEventStorage(storageProviderName);
            if (eventStorage == null)
            {
                throw new ArgumentNullException($"A storage provider that did not find  {storageProviderName}, requested to approve  {storageProviderName}'s storage provider");
            }
            this.logger.LogInformation($"{this.config.EventSourceName} Warehousing provider {storageProviderName} used by the event source ");
            this.eventBufferBlock = eventBufferBlockFactory.Create(storageProviderName, this.config.EventSourceName, this.eventStorage);
        }
        public async Task<List<IEvent<TStateKey>>> GetListAsync(EventQueryModel queryModel)
        {
            queryModel.StateId = stateKey.ToString();
            List<string> tables = await this.storageSharding.GetTableList(stateKey, queryModel.StartTime);
            List<IEvent<TStateKey>> events = new List<IEvent<TStateKey>>();
            foreach (var t in tables)
            {
                var eventModel = await eventStorage.GetListAsync(config.EventSourceName, queryModel);
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
                        this.logger.LogWarning($"{model.TypeCode}.{model.Version}  not equal to {typeof(IEvent<TStateKey>).Name}");
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
        public async Task<bool> SaveAsync(IEvent<TStateKey> @event)
        {
            string storageTableName = await this.storageSharding.GetTable(this.stateKey);
            if (string.IsNullOrEmpty(storageTableName))
            {
                throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
            }
            //Sharding processing
            EventSingleStorageModel storageModel = new EventSingleStorageModel(@event.StateId.ToString(), @event, this.config.EventSourceName, storageTableName);
            return await this.eventBufferBlock.SendAsync(storageModel);
        }
        public async Task<bool> SaveAsync(IList<IEvent<TStateKey>> events)
        {
            string storageTableName = await this.storageSharding.GetTable(this.stateKey);
            if (string.IsNullOrEmpty(storageTableName))
            {
                throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
            }
            EventCollectionStorageModel storageModel = new EventCollectionStorageModel(this.config.EventSourceName, storageTableName);
            foreach (var e in events)
            {
                EventStorageModel eventModel = new EventStorageModel(e.StateId.ToString(), e);
                storageModel.Events.Add(eventModel);
            }
            return await this.eventBufferBlock.SendAsync(storageModel);
        }
    }
}
