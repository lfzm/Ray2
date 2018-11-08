using Microsoft.Extensions.Configuration;
using Orleans.Runtime;
using Ray2;
using Ray2.PostgreSQL;
using Ray2.Storage;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PostgreSqlRayBuilderExpansions
    {
        /// <summary>
        /// Add PostgreSql Ray storage
        /// </summary>
        /// <param name="build"><see cref="IRayBuilder>"/></param>
        /// <param name="name">Provider name</param>
        /// <param name="configuration">PostgreSql configuration</param>
        /// <returns></returns>
        public static IRayBuilder AddPostgreSQL(this IRayBuilder build, string name, IConfiguration configuration)
        {
            build.Services.Configure<PostgreSqlOptions>(name, configuration);
            return build.AddPostgreSQL(name);
        }
        /// <summary>
        /// Add PostgreSql Ray storage
        /// </summary>
        /// <param name="build"><see cref="IRayBuilder>"/></param>
        /// <param name="name">Provider name</param>
        /// <param name="configuration">PostgreSql configuration</param>
        /// <returns></returns>
        public static IRayBuilder AddPostgreSQL(this IRayBuilder build, string name, Action<PostgreSqlOptions> configuration)
        {
            build.Services.Configure<PostgreSqlOptions>(name, configuration);
            return build.AddPostgreSQL(name);
        }

        /// <summary>
        /// Add PostgreSql Ray storage
        /// </summary>
        /// <param name="build"><see cref="IRayBuilder>"/></param>
        /// <param name="name">Provider name</param>
        /// <returns></returns>
        private static IRayBuilder AddPostgreSQL(this IRayBuilder build, string name)
        {
            build.Services.AddSingletonNamedService<IStateStorage>(name, (sp, n) =>
            {
                return new PostgreSqlStateStorageDecorator(sp, n);
            });
            build.Services.AddSingletonNamedService<IEventStorage>(name, (sp, n) =>
            {
                return new PostgreSqlEventStorageDecorator(sp, n);
            });
            build.Services.AddSingletonNamedService<IPostgreSqlTableStorage>(name, (sp, n) =>
            {
                return new PostgreSqlTableStorage(sp, n);
            });
            return build;
        }
    }
}
