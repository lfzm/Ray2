using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Ray2.Serialization;
using Ray2.Storage;
using System;

namespace Ray2.PostgreSQL.Test
{
    public static class FakeConfig
    {
        public const string ConnectionString = "Server=localhost;Port=5432;Database=ray2;User Id=postgres; Password=sapass;Pooling=true;MaxPoolSize=50;Timeout=10;";


        public const string ProviderName = "Default";

        public static PostgreSqlOptions Options = new PostgreSqlOptions()
        {
            ConnectionString = FakeConfig.ConnectionString
        };

        public static IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions().Configure<PostgreSqlOptions>(ProviderName, opt =>
            {
                opt.ConnectionString = Options.ConnectionString;
            });
            services.AddLogging();
            services.AddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>));
            services.AddSingletonNamedService<ISerializer, JsonSerializer>(SerializationType.JsonUTF8);
            services.AddSingletonNamedService<IStateStorage>(ProviderName, (sp, n) =>
            {
                return new PostgreSqlStateStorageDecorator(sp, n);
            });
            services.AddSingletonNamedService<IEventStorage>(ProviderName, (sp, n) =>
            {
                return new PostgreSqlEventStorageDecorator(sp, n);
            });
            services.AddSingletonNamedService<IPostgreSqlTableStorage>(ProviderName, (sp, n) =>
            {
                return new PostgreSqlTableStorage(sp, n);
            });
            return services.BuildServiceProvider();
        }
    }
}
