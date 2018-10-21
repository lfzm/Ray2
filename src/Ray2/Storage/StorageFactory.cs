using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Orleans.Runtime;
using Ray2.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public class StorageFactory : IStorageFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly StorageOptions _storageOptions;
        private readonly IStorageSharding storageSharding;
        private readonly ILogger _logger;

        public StorageFactory(IServiceProvider serviceProvider, StorageOptions storageOptions)
        {
            this._serviceProvider = serviceProvider;
        }

        public IEventStorage GetEventStorage(string eventSourceName, string stateKey)
        {
            return this._serviceProvider.GetRequiredServiceByName<IEventStorage>(eventSourceName);
        }

        public IList<IEventStorage> GetEventStorageList(string eventSourceName)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetEventTable(string eventSourceName, string stateKey)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetEventTableList(string eventSourceName, string stateKey, long? createTime)
        {
            throw new NotImplementedException();
        }

        public IStateStorage GetSnapshotStorage(string eventSourceName, string stateKey)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSnapshotTable(string eventSourceName, string stateKey)
        {
            throw new NotImplementedException();
        }
 
        public IStateStorage GetStateStorage(string eventProcessorName, string stateKey)
        {
            return this._serviceProvider.GetRequiredServiceByName<IStateStorage>(eventProcessorName);
        }

        public Task<string> GetStateTable(string eventProcessorName, string stateKey)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 获取事件源快照存储表名，调用分表策略进行分表。
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetSnapshotTableName()
        {
            if (!string.IsNullOrEmpty(this.SnapshotTableName))
                return this.SnapshotTableName;

            string storageTableName = string.Empty;
            if (this._snapshotStorageSharding != null)
            {
                storageTableName = await this._snapshotStorageSharding.GetTable(this.Options.EventSourceName, StorageType.EventSourceSnapshot, this.StateId.ToString());
                if (string.IsNullOrEmpty(storageTableName))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                storageTableName = this.Options.EventSourceName;
            }
            this.SnapshotTableName = StorageTableNameBuild.BuildSnapshotTableName(storageTableName);
            return this.SnapshotTableName;
        }
        /// <summary>
        /// 获取事件源存储表名，调用分表策略进行分表。
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetEventTableName()
        {
            string storageTableName = string.Empty;
            if (this._eventStorageSharding != null)
            {
                storageTableName = await this._eventStorageSharding.GetTable(this.Options.EventSourceName, StorageType.EventSource, this.StateId.ToString());
                if (string.IsNullOrEmpty(storageTableName))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                storageTableName = this.Options.EventSourceName;
            }
            return StorageTableNameBuild.BuildEventTableName(storageTableName);
        }
        /// <summary>
        /// 获取事件源所有存储表名，调用分表策略进行分表。
        /// </summary>
        /// <returns></returns>
        private async Task<List<string>> GetEventTableNameList(long? createTime)
        {
            List<string> tables = new List<string>();
            if (this._eventStorageSharding != null)
            {
                tables = await this._eventStorageSharding.GetTableList(this.Options.EventSourceName, StorageType.EventSource, this.StateId.ToString(), createTime);
                if (tables == null || tables.Count == 0)
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                tables.Add(this.Options.EventSourceName);
            }
            return tables.Select(f => StorageTableNameBuild.BuildEventTableName(f)).ToList();
        }
        /// <summary>
        /// Get a event sourcing  snapshot storage provider
        /// </summary>
        /// <returns></returns>
        private async Task<IStateStorage> GetSnapshotStorageProvider()
        {
            string storageProvider = string.Empty;
            if (!string.IsNullOrEmpty(this.Options.SnapshotOptions.ShardingStrategy))
            {
                this._snapshotStorageSharding = this._serviceProvider.GetRequiredServiceByName<IStorageSharding>(this.Options.SnapshotOptions.ShardingStrategy);
                storageProvider = await this._snapshotStorageSharding.GetProvider(this.Options.EventSourceName, StorageType.EventSourceSnapshot, this.StateId.ToString());
                if (string.IsNullOrEmpty(storageProvider))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                storageProvider = this.Options.SnapshotOptions.StorageProvider;
            }

            var sp = this._storageFactory.GetStateStorage(storageProvider);
            return sp;
        }
        /// <summary>
        /// Get a event sourcing  storage provider
        /// </summary>
        /// <returns></returns>
        private async Task<IEventStorage> GetEventStorageProvider()
        {
            string storageProvider = string.Empty;
            if (!string.IsNullOrEmpty(this.Options.StorageOptions.ShardingStrategy))
            {
                this._eventStorageSharding = this._serviceProvider.GetRequiredServiceByName<IStorageSharding>(this.Options.StorageOptions.ShardingStrategy);
                storageProvider = await this._eventStorageSharding.GetProvider(this.Options.EventSourceName, StorageType.EventSource, this.StateId.ToString());
                if (string.IsNullOrEmpty(storageProvider))
                {
                    throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
                }
            }
            else
            {
                storageProvider = this.Options.SnapshotOptions.StorageProvider;
            }
            var sp = this._storageFactory.GetEventStorage(storageProvider);
            return sp;
        }

        Task<IEventStorage> IStorageFactory.GetEventStorage(string eventSourceName, string stateKey)
        {
            throw new NotImplementedException();
        }

        Task<IList<IEventStorage>> IStorageFactory.GetEventStorageList(string eventSourceName)
        {
            throw new NotImplementedException();
        }

        Task<IStateStorage> IStorageFactory.GetSnapshotStorage(string eventSourceName, string stateKey)
        {
            throw new NotImplementedException();
        }

        Task<IStateStorage> IStorageFactory.GetStateStorage(string eventProcessorName, string stateKey)
        {
            throw new NotImplementedException();
        }
    }
}
