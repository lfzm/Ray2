using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Orleans.Runtime;
using Ray2.Configuration;

namespace Ray2.Storage
{
    public class DefaultStorageFactory : IStorageFactory
    {
        public readonly IServiceProvider serviceProvider;

        public DefaultStorageFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEventStorage GetEventStorage(string name)
        {
            return this.serviceProvider.GetRequiredServiceByName<IEventStorage>(name);
        }

        public IStatusStorage GetStatusStorage(string name)
        {
            return this.serviceProvider.GetRequiredServiceByName<IStatusStorage>(name);
        }

       
    }
}
