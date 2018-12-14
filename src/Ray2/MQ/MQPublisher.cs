using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray2.Configuration;
using Ray2.EventSource;
using Ray2.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ray2.MQ
{
    public class MQPublisher : IMQPublisher
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataflowBufferBlockFactory _dataflowBufferBlockFactory;

        public MQPublisher(IServiceProvider serviceProvider, IDataflowBufferBlockFactory dataflowBufferBlockFactory, ILogger<MQPublisher> logger)
        {
            this._serviceProvider = serviceProvider;
            this._dataflowBufferBlockFactory = dataflowBufferBlockFactory;
            this._logger = logger;
        }

        public Task<bool> PublishAsync(IEvent evt, string topic, string mqProviderName, MQPublishType publishType = MQPublishType.Asynchronous)
        {
            EventPublishBufferWrap wrap = new EventPublishBufferWrap(evt, topic, mqProviderName, publishType);
            return this.PublishAsync(wrap);
        }

        public Task<bool> PublishAsync(EventPublishBufferWrap warp)
        {
            if (warp.Type == MQPublishType.NotPublish)
            {
                return Task.FromResult(false);
            }
            else
            {
                var bufferBlock = _dataflowBufferBlockFactory.Create<EventPublishBufferWrap>(warp.MQProviderName, this.LazyPublishAsync);
                if (warp.Type == MQPublishType.Synchronous)
                {
                    return bufferBlock.SendAsync(warp);
                }
                else
                {
                    return bufferBlock.SendAsync(warp, false);
                }
            }
        }

        public Task LazyPublishAsync(BufferBlock<EventPublishBufferWrap> eventBuffer)
        {
            List<EventPublishBufferWrap> eventWraps = new List<EventPublishBufferWrap>();
            while (eventBuffer.TryReceive(out var model))
            {
                eventWraps.Add(model);
                if (eventWraps.Count >= 1000)
                    break;
            }
            if (eventWraps.Count == 0)
                return Task.CompletedTask;
            //this._logger.LogError("处理条数" + eventWraps.Count);
            var provider = this._serviceProvider.GetRequiredServiceByName<IEventPublisher>(eventWraps[0].MQProviderName);
            eventWraps.ForEach(warp => warp.Result = this.PublishAsync(provider, warp.Topic, new EventModel(warp.Value)));
            Task.WaitAll(eventWraps.Select(f => f.Result).ToArray());
            eventWraps.ForEach(warp => warp.TaskSource.SetResult(warp.Result.Result));
            return Task.CompletedTask;
        }

        private async Task<bool> PublishAsync(IEventPublisher publisher, string topic, EventModel model)
        {
            try
            {
                return await publisher.Publish(topic, model);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, $"Publishing {topic} event failed");
                return false;
            }

        }

    }
}
