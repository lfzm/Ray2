using Ray2.Configuration.Builder;
using System;
using System.Reflection;

namespace Ray2.Configuration.Creator
{
    public class EventSourceOptionsCreator : IEventSourceOptionsCreator
    {
        public EventSourceOptions Create(Type type)
        {
            var attr = type.GetCustomAttribute<EventSourcingAttribute>();
            if (attr == null)
            {
                throw new Exception($"The {type.FullName}  does not have an {nameof(EventSourcingAttribute)} configured.");
            }

            var snapshotOptions = this.CreateSnapshotOptions(attr);
            var storageOptions = this.CreateStorageOptions(attr);
            string name = attr.Name;
            if (string.IsNullOrEmpty(name))
            {
                name = type.Name;
            }

            return new EventSourceOptionsBuilder().WithEventSourcing(name, type)
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
