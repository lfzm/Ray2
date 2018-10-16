using Microsoft.Extensions.Logging;
using Ray2.Storage;
using System;
using System.Collections.Concurrent;

namespace Ray2.EventSourcing
{
    public class EventBufferBlockFactory : IEventBufferBlockFactory
    {
        private readonly ILoggerFactory loggerFactory;
        ConcurrentDictionary<string, EventBufferBlock> EventBufferBlocks = new ConcurrentDictionary<string, EventBufferBlock>();
        public EventBufferBlockFactory(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }
        public IEventBufferBlock Create(string storageProviderName, string eventSourcing, IEventStorage eventStorage)
        {
            return EventBufferBlocks.GetOrAdd($"{storageProviderName}_{eventSourcing}", (key) =>
            {
               
                return new EventBufferBlock(storageProviderName, loggerFactory.CreateLogger<EventBufferBlock>(), eventStorage);
            });
        }
    }
}
