using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Configuration.Builder
{
    public class EventSourceOptionsBuilder
    {
        private string _eventSourceName;
        private string _fourcingFullName;
        private SnapshotOptions _snapshotOptions;
        private StorageOptions _storageOptions;

        public EventSourceOptionsBuilder WithEventSourcing(string name, Type type)
        {
            this._eventSourceName = name;
            this._fourcingFullName = type.FullName;
            return this;
        }

        public EventSourceOptionsBuilder WithSnapshotOptions(SnapshotOptions snapshotOptions)
        {
            this._snapshotOptions = snapshotOptions;
            return this;
        }

        public EventSourceOptionsBuilder WithStorageOptions(StorageOptions storageOptions)
        {
            this._storageOptions = storageOptions;
            return this;
        }
        public EventSourceOptions Build()
        {
            return new EventSourceOptions(_eventSourceName, _fourcingFullName, _snapshotOptions, _storageOptions);
        }
    }
}
