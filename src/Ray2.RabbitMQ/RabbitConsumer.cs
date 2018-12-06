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
        public bool NeedRestart { get; private set; }
        public RabbitConsumer(IServiceProvider serviceProvider, string providerName, ISerializer serializer)
        {
            this.providerName = providerName;
            this._serviceProvider = serviceProvider;
            this._serializer = serializer;
            this._internalConfiguration = serviceProvider.GetRequiredService<IInternalConfiguration>();
            this._logger = serviceProvider.GetRequiredService<ILogger<RabbitConsumer>>();
            var _channelFactory = serviceProvider.GetRequiredServiceByName<IRabbitChannelFactory>(this.providerName);
            this._channel = _channelFactory.GetChannel();
        }
        public Task Close()
        {
            this._channel.Close();
            return Task.CompletedTask;
        }
        public bool IsAvailable()
        {
            //Restart the queue if there is an exception in the callback
            if (this.NeedRestart)
                return true;
            if (!this._channel.IsOpen())
                return true;
            else
                return false;
        }
        public bool IsExpand()
        {
            //Expand to maximum queue if automatically confirmed
            if (this.Options.AutoAck)
            {
                return true;
            }
            if (this._channel.IsOpen())
            {
                //Expand the number of stacks by more than twice the number of treatments
                uint accumulation = this._channel.MessageCount(this.Queue);
                if (accumulation >= this.Options.OneFetchCount * 2)
                {
                    return true;
                }
            }
            return false;
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
            this._channel.Model.CallbackException += Channel_CallbackException;

            //Consumer reports in the channel
            var basicConsumer = new EventingBasicConsumer(this._channel.Model);
            basicConsumer.Received += async (ch, ea) =>
            {
                await Process(ea);
            };
            basicConsumer.ConsumerTag = this._channel.Model.BasicConsume(this.Queue, options.AutoAck, basicConsumer);
            return Task.CompletedTask;
        }
      

        public void Channel_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            this._logger.LogError(e.Exception.InnerException ?? e.Exception, $"RabbitMQ callback exception; {this.Exchange}-{this.Queue}");
            this.NeedRestart = true;

        }
        public async Task Process(BasicDeliverEventArgs e)
        {
            var model = this.ConversionEvent(e.Body);
            if (model != null)
            {
                await this.Process(model, 0);
            }
            if (!this.Options.AutoAck)
            {
                try
                {
                    this._channel.Model.BasicAck(e.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex.InnerException ?? ex, $"RabbitMQ confirms receipt of message failed; {this.Exchange}-{this.Queue}");
                }
            }
        }
        public async Task Process(EventModel model, int count)
        {
            try
            {
                if (count > 0)
                    await Task.Delay(count * 1000);

                await this.Processor.Tell(model);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.InnerException ?? ex, $"Notification processor processing failed; {this.Exchange}-{this.Queue}");
                if (this.Options.NoticeRetriesCount > count)
                {
                    await Process(model, count + 1);
                }
            }
        }
        public EventModel ConversionEvent(byte[] bytes)
        {
            try
            {
                var message = this._serializer.Deserialize<PublishMessage>(bytes);
                //Get event type
                if (this._internalConfiguration.GetEvenType(message.TypeCode, out Type type))
                {
                    object data = this._serializer.Deserialize(type, message.Data);
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
                this._logger.LogError(ex, $"Message serialization failed in {this.Exchange}-{this.Queue}");
                return null;
            }
        }
    }
}
