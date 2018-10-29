using FluentValidation;
using Orleans.Runtime;
using Ray2.Storage;
using System;
using System.Linq;

namespace Ray2.Configuration.Validator
{
    public class EventProcessOptionsFluentVaildator : AbstractValidator<EventProcessOptions>
    {
        private readonly IServiceProvider serviceProvider;

        public EventProcessOptionsFluentVaildator(IServiceProvider provider, EventSubscribeOptionsFluentVaildator eventSubscribeOptionsFluentVaildator)
        {
            this.serviceProvider = provider;

            base.RuleFor(x => x.ProcessorName)
                .NotEmpty()
                .WithMessage(x => $"EventProcessorAttribute.Name in {x.ProcessorFullName} cannot be empty");

            base.When(x => x.ProcessorType == ProcessorType.GrainProcessor, () =>
               {
                   base.RuleFor(x => x.EventSourceName)
                       .NotEmpty()
                       .WithMessage(x => $"EventProcessorAttribute.EventSourceName in {x.ProcessorFullName} cannot be empty");
               });

            base.RuleFor(x => x.OnceProcessCount)
                .GreaterThan(0)
                .WithMessage(x => $"The EventProcessorAttribute.OnceProcessCount configuration in {x.ProcessorFullName} must be greater than zero");

            base.RuleFor(x => x.StatusOptions)
                      .Must(x => string.IsNullOrEmpty(x.ShardingStrategy) && string.IsNullOrEmpty(x.StorageProvider))
                      .WithMessage(x => $" Need to configure EventProcessorAttribute.StorageProvider or EventProcessorAttribute.ShardingStrategy storage provider in {x.ProcessorFullName}");
            base.When(x => !string.IsNullOrEmpty(x.StatusOptions.ShardingStrategy), () =>
            {
                base.RuleFor(x => x.StatusOptions.ShardingStrategy)
                    .Must(this.HavaShardingStrategyRegistered)
                    .WithMessage("{PropertyValue} IStorageSharding is not injected into the Ray");
            });
            base.When(x => !string.IsNullOrEmpty(x.StatusOptions.StorageProvider), () =>
            {
                base.RuleFor(x => x.StatusOptions.StorageProvider)
                    .Must(this.HavaStateStorageProviderRegistered)
                    .WithMessage("{PropertyValue} IEventStorage provider is not injected into the Ray");
            });

            base.RuleFor(x => x.SubscribeOptions.Count())
                .GreaterThan(0)
                .WithMessage(x => $"{x.ProcessorFullName} is not configured with EventSubscribeAttribute and cannot be subscribed to");
            base.RuleForEach(x => x.SubscribeOptions)
                .SetValidator(eventSubscribeOptionsFluentVaildator);

        }

        private bool HavaStateStorageProviderRegistered(string storageProvider)
        {
            var provider = this.serviceProvider.GetServiceByName<IStateStorage>(storageProvider);
            return provider != null;
        }

        private bool HavaShardingStrategyRegistered(string shardingStrategy)
        {
            var provider = this.serviceProvider.GetServiceByName<IStorageSharding>(shardingStrategy);
            return provider != null;
        }
    }
}
