using Microsoft.Extensions.Logging;
using Ray2.Storage;
using System;
using System.Collections.Concurrent;

namespace Ray2.EventSources
{
    public class EventBufferBlockFactory : IEventBufferBlockFactory
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly IStorageFactory storageFactory;
        ConcurrentDictionary<string, EventBufferBlock> EventBufferBlocks = new ConcurrentDictionary<string, EventBufferBlock>();
        public EventBufferBlockFactory(ILoggerFactory loggerFactory, IStorageFactory storageFactory)
        {
            this.loggerFactory = loggerFactory;
            this.storageFactory = storageFactory;
        }
        public IEventBufferBlock Create(string storageProviderName, string eventSourceName,IEventStorage  eventStorage)
        {
            return EventBufferBlocks.GetOrAdd($"{storageProviderName}_{eventSourceName}", (key) =>
            {
                
                return new EventBufferBlock(storageProviderName, loggerFactory.CreateLogger<EventBufferBlock>(), eventStorage);
            });
        }
    }
}
