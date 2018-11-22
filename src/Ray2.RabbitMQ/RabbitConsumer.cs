using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ray2.Configuration;
using Ray2.EventProcess;
using Ray2.EventSource;
using Ray2.RabbitMQ.Configuration;
using Ray2.Serialization;
using System;
using System.Threading.Tasks;

namespace Ray2.RabbitMQ
{
    public class RabbitConsumer : IRabbitConsumer
    {
        private readonly string providerName;
        private readonly IServiceProvider _serviceProvider;
        private readonly IInternalConfiguration _internalConfiguration;
        private readonly ILogger _logger;
        private readonly IRabbitChannel _channel;
        private readonly ISerializer _serializer;
        public string Queue { get; private set; }
        public string Exchange { get; private set; }
        public RabbitConsumeOptions Options { get; private set; }
        public IEventProcessor Processor { get; private set; }
        public RabbitConsumer(string providerName, IServiceProvider serviceProvider, ISerializer serializer)
        {
            this.providerName = providerName;
            this._serviceProvider = serviceProvider;
            this._serializer = serializer;
            this._internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            this._logger = serviceProvider.GetRequiredService<ILogger<RabbitConsumer>>();
            var _channelPool = serviceProvider.GetRequiredServiceByName<IRabbitChannelPool>(this.providerName);
            this._channel = _channelPool.GetChannel().GetAwaiter().GetResult();
        }

        public Task<bool> IsAvailable()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsExpand()
        {
            throw new NotImplementedException();
        }

        public Task Subscribe(string queue, string exchange, IEventProcessor processor, RabbitConsumeOptions options)
        {
            this.Exchange = exchange;
            this.Queue = queue;
            this.Processor = processor;
            this.Options = options;

            //Subscription processing
            this._channel.Model.ExchangeDeclare(this.Exchange, ExchangeType.Direct, true);
            this._channel.Model.QueueDeclare(this.Queue, true, false, false, null);
            this._channel.Model.QueueBind(this.Queue, this.Exchange, this.Exchange);
            this._channel.Model.BasicQos(0, options.OneFetchCount, false);

            var basicConsumer = new EventingBasicConsumer(this._channel.Model);
            basicConsumer.Received += async (ch, ea) =>
            {
                await Process(ea, 0);
            };
            basicConsumer.ConsumerTag = this._channel.Model.BasicConsume(this.Queue, options.AutoAck, basicConsumer);
            return Task.CompletedTask;
        }

        public async Task Process(BasicDeliverEventArgs e, int count)
        {
            if (count > 0)
                await Task.Delay(count * 1000);
            try
            {
                var model = this.ConversionEvent(e.Body);
                if (model != null)
                {
                    await this.Processor.Tell(model);
                }
                if (!this.Options.AutoAck)
                {
                    try
                    {
                        this._channel.Model.BasicAck(e.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {

                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.InnerException ?? ex, $"An error occurred in {this.Exchange}-{this.Queue}");
                if (count > 3)
                    consumerChild.NeedRestart = true;
                else
                    await Process(e, count + 1);
            }
        }

        public EventModel ConversionEvent(byte[] bytes)
        {
            try
            {
                var message = this._serializer.Deserialize<EventPublishMessage>(bytes);
                //Get event type
                if (this._internalConfiguration.GetEvenType(message.TypeCode, out Type type))
                {
                    object data = this._serializer.Deserialize(type, bytes);
                    if (data is IEvent e)
                    {
                        EventModel eventModel = new EventModel(e, message.TypeCode, message.Version);
                        return eventModel;
                    }
                    else
                    {
                        this._logger.LogWarning($"{message.TypeCode}.{message.Version}  not equal to IEvent");
                        return null;
                    }
                }
                else
                {
                    this._logger.LogWarning($"{message.TypeCode}.{message.Version} event type not exist to IInternalConfiguration");
                    return null;
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex,$"Message serialization failed in {this.Exchange}-{this.Queue}");
                return null;
            }
        }


    }
}
