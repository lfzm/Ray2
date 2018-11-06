using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Ray2.Serialization;
using Ray2.Storage;
using System;

namespace Ray2.PostgreSQL.Test
{
    public static class FakeConfig
    {
        public const string ConnectionString = "Server=localhost;Port=5432;Database=Ray2;User Id=postgres; Password=sapass;Pooling=true;MaxPoolSize=50;Timeout=10;";

       
        public const string ProviderName = "Default";

        public static IServiceProvider BuildServiceProvider(SerializationType type)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions().Configure<PostgreSqlOptions>(ProviderName, opt =>
            {
                opt.ConnectionString = FakeConfig.ConnectionString;
                opt.SerializationType = type;
            });
            services.AddLogging();
            services.AddTransient<IStringSerializer, JsonSerializer>();
            services.AddTransient<IByteSerializer, ProtobufSerializer>();
            services.AddTransient<ISerializer, Serializer>();

            services.AddSingletonNamedService<IEventStorage>(ProviderName, (sp, n) =>
            {
                return new PostgreSqlEventStorage(sp, n);
            });
            services.AddSingletonNamedService<IStateStorage>(ProviderName, (sp, n) =>
            {
                return new PostgreSqlStateStorage(sp, n);
            });
            services.AddSingletonNamedService<IPostgreSqlTableStorage>(ProviderName, (sp, n) =>
            {
                return new PostgreSqlTableStorage(sp, n);
            });
            return services.BuildServiceProvider();
        }
    }
}
