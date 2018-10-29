using FluentValidation;
using Orleans.Runtime;
using Ray2.Storage;
using System;

namespace Ray2.Configuration.Validator
{
    public class EventSourceOptionsFluentVaildator : AbstractValidator<EventSourceOptions>
    {
        private readonly IServiceProvider serviceProvider;
        public EventSourceOptionsFluentVaildator(IServiceProvider provider)
        {
            this.serviceProvider = provider;

            base.RuleFor(x => x.EventSourceName)
                .NotEmpty()
                .WithMessage(x => $"EventSourcingAttribute.Name in {x.SourcingFullName} cannot be empty");

            base.RuleFor(x => x.StorageOptions)
                      .Must(x => string.IsNullOrEmpty(x.ShardingStrategy) && string.IsNullOrEmpty(x.StorageProvider))
                      .WithMessage(x => $" Need to configure EventSourcingAttribute.StorageProvider or EventSourcingAttribute.ShardingStrategy storage provider in {x.SourcingFullName}");
            base.When(x => !string.IsNullOrEmpty(x.StorageOptions.ShardingStrategy), () =>
            {
                base.RuleFor(x => x.StorageOptions.ShardingStrategy)
                    .Must(this.HavaShardingStrategyRegistered)
                    .WithMessage("{PropertyValue} IStorageSharding is not injected into the Ray");
            });
            base.When(x => !string.IsNullOrEmpty(x.StorageOptions.StorageProvider), () =>
            {
                base.RuleFor(x => x.StorageOptions.StorageProvider)
                    .Must(this.HavaEventStorageProviderRegistered)
                    .WithMessage("{PropertyValue} IEventStorage provider is not injected into the Ray");
            });

            base.When(x => x.SnapshotOptions.SnapshotType != SnapshotType.NoSnapshot, () =>
               {
                   base.RuleFor(x => x.SnapshotOptions)
                       .Must(x => string.IsNullOrEmpty(x.ShardingStrategy) && string.IsNullOrEmpty(x.StorageProvider))
                       .WithMessage(x => $" Need to configure EventSourcingAttribute.StorageProvider or EventSourcingAttribute.ShardingStrategy storage provider in {x.SourcingFullName}");

                   base.When(x => !string.IsNullOrEmpty(x.SnapshotOptions.ShardingStrategy), () =>
                   {
                       base.RuleFor(x => x.SnapshotOptions.ShardingStrategy)
                           .Must(this.HavaShardingStrategyRegistered)
                           .WithMessage("{PropertyValue} IStorageSharding is not injected into the Ray");
                   });
                   base.When(x => !string.IsNullOrEmpty(x.SnapshotOptions.StorageProvider), () =>
                   {
                       base.RuleFor(x => x.SnapshotOptions.StorageProvider)
                           .Must(this.HavaSnapshotStorageProviderRegistered)
                           .WithMessage("{PropertyValue} IStateStorage provider is not injected into the Ray");
                   });
               });
        }

        private bool HavaEventStorageProviderRegistered(string storageProvider)
        {
            var provider = this.serviceProvider.GetServiceByName<IEventStorage>(storageProvider);
            return provider != null;
        }

        private bool HavaSnapshotStorageProviderRegistered(string storageProvider)
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
