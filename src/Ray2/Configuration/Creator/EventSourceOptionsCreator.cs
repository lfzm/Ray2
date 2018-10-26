using Ray2.Configuration.Builder;
using System;
using System.Reflection;

namespace Ray2.Configuration.Creator
{
    public class EventSourceOptionsCreator : IEventSourceOptionsCreator
    {
        private readonly EventSourceOptionsBuilder _builder;
        public EventSourceOptionsCreator(EventSourceOptionsBuilder builder)
        {
            this._builder = builder;
        }
        public EventSourceOptions Create(Type type)
        {
            var attr = type.GetCustomAttribute<EventSourcingAttribute>();
            if (attr == null)
            {
                throw new Exception($"The {type.FullName}  does not have an EventSourcingAttribute configured.");
            }

            var snapshotOptions = this.CreateSnapshotOptions(attr);
            var storageOptions = this.CreateStorageOptions(attr);

            return new EventSourceOptionsBuilder().WithEventSourcing(attr.Name, type)
                   .WithSnapshotOptions(snapshotOptions)
                   .WithStorageOptions(storageOptions)
                   .Build();
        }

        private SnapshotOptions CreateSnapshotOptions(EventSourcingAttribute attribute)
        {
            if (attribute.SnapshotType == SnapshotType.NoSnapshot)
            {
                return new SnapshotOptions();
            }
            else
            {
                return new SnapshotOptions(attribute.StorageProvider, attribute.ShardingStrategy, attribute.SnapshotType);
            }
        }
        private StorageOptions CreateStorageOptions(EventSourcingAttribute attribute)
        {
            return new StorageOptions(attribute.StorageProvider, attribute.ShardingStrategy);
        }
    }
}
