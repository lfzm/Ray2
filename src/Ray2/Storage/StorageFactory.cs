using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Ray2.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ray2.Storage
{
    public class StorageFactory : IStorageFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly StorageOptions _options;
        private readonly ILogger _logger;
        private readonly IStorageSharding _storageSharding;

        public StorageFactory(IServiceProvider serviceProvider, StorageOptions storageOptions)
        {
            this._options = storageOptions;
            this._serviceProvider = serviceProvider;
            this._logger = this._serviceProvider.GetRequiredService<ILogger<StorageFactory>>();
            this._storageSharding = this.GetStorageSharding();
        }

        public async Task<IEventStorage> GetEventStorage(string name, string stateKey)
        {
            string storageProvider = string.Empty;
            if (this._storageSharding != null)
            {
                storageProvider = await this._storageSharding.GetProvider(name, StorageType.EventSource, stateKey);
            }
            else
            {
                storageProvider = this._options.StorageProvider;
            }
            if (string.IsNullOrEmpty(storageProvider))
            {
                throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
            }
            return this._serviceProvider.GetRequiredServiceByName<IEventStorage>(storageProvider);
        }

        public async Task<List<EventStorageInfo>> GetEventStorageList(string name, long? createTime)
        {
            List<EventStorageInfo> storageList = new List<EventStorageInfo>();
            if (this._storageSharding != null)
            {
                storageList = await this._storageSharding.GetProviderList(name, StorageType.EventSource, createTime);
            }
            else
            {
                EventStorageInfo storageInfo = new EventStorageInfo();
                storageInfo.EventSource = name;
                storageInfo.Tables = new List<string>();
                storageInfo.Tables.Add(name);
            }
            if (storageList == null || storageList.Count == 0)
            {
                throw new ArgumentNullException($"{name} Event source has no storage provider.");
            }
            for (int i = 0; i < storageList.Count; i++)
            {
                EventStorageInfo storage = storageList[i];
                if (storage.Tables == null || storage.Tables.Count == 0)
                {
                    throw new ArgumentNullException($"{name} Event source does not have a corresponding storage table.");
                }
                storage.Storage = this._serviceProvider.GetRequiredServiceByName<IEventStorage>(storage.Provider);
            }
            return storageList;
        }

        public async Task<IStateStorage> GetStateStorage(string name, StorageType storageType, string stateKey)
        {
            if (storageType == StorageType.EventSource)
            {
                throw new ArgumentNullException("The EventSource store cannot be used with IStateStorage , please call GetEventStorage to get IEventStorage for storage.");
            }
            string storageProvider = string.Empty;
            if (this._storageSharding != null)
            {
                storageProvider = await this._storageSharding.GetProvider(name, storageType, stateKey);
            }
            else
            {
                storageProvider = this._options.StorageProvider;
            }
            if (string.IsNullOrEmpty(storageProvider))
            {
                throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
            }
            return this._serviceProvider.GetRequiredServiceByName<IStateStorage>(storageProvider);
        }

        public async Task<string> GetTable(string name, StorageType storageType, string stateKey)
        {
            string storageTableName = string.Empty;
            if (this._storageSharding != null)
            {
                storageTableName = await this._storageSharding.GetTable(name, storageType, stateKey);
            }
            else
            {
                storageTableName = name;
            }
            if (string.IsNullOrEmpty(storageTableName))
            {
                throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
            }
            return StorageTableNameBuild.BuildTableName(storageTableName, storageType);
        }

        public async Task<List<string>> GetTableList(string name, StorageType storageType, string stateKey, long? createTime)
        {
            List<string> tables = new List<string>();
            if (this._storageSharding != null)
            {
                tables = await this._storageSharding.GetTableList(name, storageType, stateKey, createTime);
            }
            else
            {
                if (!string.IsNullOrEmpty(name))
                    tables.Add(name);
            }
            if (tables == null || tables.Count == 0)
            {
                throw new ArgumentNullException("Get storage table name from IStorageSharding cannot be empty");
            }
            return tables.Select(f => StorageTableNameBuild.BuildTableName(f, storageType)).ToList();
        }

        private IStorageSharding GetStorageSharding()
        {
            if (string.IsNullOrEmpty(this._options.ShardingStrategy))
                return null;
            return this._serviceProvider.GetRequiredServiceByName<IStorageSharding>(this._options.ShardingStrategy);
        }
    }
}
