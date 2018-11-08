using System;
using System.Collections.Generic;
using System.Text;

namespace Ray2.Storage
{
    public interface IEventStorageModel
    {
        string EventSourceName { get; }
        string StorageTableName { get; }

        object GetStateId();
    }
}
