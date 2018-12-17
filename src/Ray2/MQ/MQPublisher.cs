using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray2.EventSource;
using Ray2.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
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
            EventPublishModel wrap = new EventPublishModel(evt, topic, mqProviderName, publishType);
            return this.PublishAsync(wrap);
        }

        public Task<bool> PublishAsync(EventPublishModel model)
        {
            if (model.Type == MQPublishType.NotPublish)
            {
                return Task.FromResult(false);
            }
            else
            {
                var bufferBlock = _dataflowBufferBlockFactory.Create<EventPublishModel>(model.MQProviderName, this.LazyPublishAsync);
                if (model.Type == MQPublishType.Synchronous)
                {
                    return bufferBlock.SendAsync(model);
                }
                else
                {
                    return bufferBlock.SendAsync(model, false);
                }
            }
        }

        public Task LazyPublishAsync(BufferBlock<IDataflowBufferWrap<EventPublishModel>> eventBuffer)
        {
            List<IDataflowBufferWrap<EventPublishModel>> eventWraps = new List<IDataflowBufferWrap<EventPublishModel>>();
            while (eventBuffer.TryReceive(out var wrap))
            {
                eventWraps.Add(wrap);
                if (eventWraps.Count >= 1000)
                    break;
            }
            if (eventWraps.Count == 0)
                return Task.CompletedTask;
            var provider = this._serviceProvider.GetRequiredServiceByName<IEventPublisher>(eventWraps[0].Data.MQProviderName);
            var tasks = eventWraps.Select(warp =>
            {
                var result = this.PublishAsync(provider, warp.Data.Topic, new EventModel(warp.Data.Event));
                return warp.CompleteHandler(result);
            }).ToList();
            Task.WaitAll(tasks.ToArray());
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
