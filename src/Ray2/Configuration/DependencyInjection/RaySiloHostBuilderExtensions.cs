using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Runtime;
using Ray2;
using Ray2.Configuration.Validator;
using Ray2.MQ;
using System;

namespace Orleans.Hosting
{
    public static class RaySiloHostBuilderExtensions
    {
        /// <summary>
        /// Use Ray
        /// </summary>
        /// <param name="hostBuilder"><see cref="ISiloHostBuilder"/></param>
        /// <param name="configuration">Ray and Ray module configuration</param>
        /// <param name="builder">Provide a action for building Ray</param>
        /// <returns></returns>
        public static ISiloHostBuilder UseRay(this ISiloHostBuilder hostBuilder, Action<IRayBuilder> builder)
        {
            hostBuilder.ConfigureServices((HostBuilderContext build, IServiceCollection services) =>
            {

                services.AddRay(build.Configuration, builder);
            });
            hostBuilder.EnableDirectClient();
            hostBuilder.AddStartupTask((sp,cancellationToken)=>
            {
                return sp.GetRequiredService<IMQSubscriber>().Start();
            });
            return hostBuilder;
        }

        /// <summary>
        /// Add Ray
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="configuration">Ray and Ray module configuration</param>
        /// <param name="builder">Provide a action for building Ray</param>
        /// <returns></returns>
        internal static IServiceCollection AddRay(this IServiceCollection services, IConfiguration configuration, Action<IRayBuilder> builder)
        {
            services.AddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>));
            services.AddLogging();
            var build = new RayBuilder(services, configuration);
            if (builder == null)
            {
                throw new RayConfigurationException("Did not inject MQ providers and Storage providers into Ray");
            }
            builder.Invoke(build);
         
            //Ray builder 
            build.Build();
            return services;
        }
    }
}
