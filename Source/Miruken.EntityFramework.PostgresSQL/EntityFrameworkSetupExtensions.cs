// ReSharper disable InconsistentNaming
namespace Miruken.EntityFramework.PostgresSQL
{
    using System;
    using Callback;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

    public static class EntityFrameworkSetupExtensions
    {
        public static EntityFrameworkSetup UsePostgresSQL<T>(
            this EntityFrameworkSetup setup)
            where T : DbContext
        {
            return setup.DbContext<UsePostgresSQL<T>>();
        }

        public static EntityFrameworkSetup UsePostgresSQL<T, TC>(
            this EntityFrameworkSetup setup)
            where T  : DbContext
            where TC : IExtension<UsePostgresSQL<T>>
        {
            return setup.DbContext<UsePostgresSQL<T>, TC>();
        }

        public static EntityFrameworkSetup UsePostgresSQL<T>(
            this EntityFrameworkSetup setup,
            Action<NpgsqlDbContextOptionsBuilder> configure)
            where T : DbContext
        {
            return configure != null
                 ? setup.DbContext<UsePostgresSQL<T>>(services => services
                     .AddSingleton(new UsePostgresSQL<T>.ActionConfiguration(configure)))
                 : setup.DbContext<UsePostgresSQL<T>>();
        }
        
        public static EntityFrameworkSetup UsePostgresSQL(
            this EntityFrameworkSetup setup,
            Type dbContextConfiguration = null)
        {
            return setup.DbContext(typeof(UsePostgresSQL<>), dbContextConfiguration);
        }
        
        public static EntityFrameworkSetup UsePostgresSQL(
            this EntityFrameworkSetup setup,
            Action<NpgsqlDbContextOptionsBuilder> configure)
        {
            return setup.DbContext(typeof(UsePostgresSQL<>), action: services =>
                services.AddSingleton(new DefaultActionConfigurationProvider(configure)));
        }

        [Unmanaged]
        private class DefaultActionConfigurationProvider : Handler
        {
            private readonly Action<NpgsqlDbContextOptionsBuilder> _configure;

            public DefaultActionConfigurationProvider(
                Action<NpgsqlDbContextOptionsBuilder> configure)
            {
                _configure = configure;
            }

            [Provides, Singleton]
            public UsePostgresSQL<T>.Configuration Get<T>()
                where T : DbContext
            {
                return (UsePostgresSQL<T>.Configuration) Activator.CreateInstance(
                    typeof(UsePostgresSQL<T>.ActionConfiguration), _configure);
            }
        }
    }
}
