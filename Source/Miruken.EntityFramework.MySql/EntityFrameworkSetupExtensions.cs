namespace Miruken.EntityFramework.MySql
{
    using System;
    using Callback;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class EntityFrameworkSetupExtensions
    {
        public static EntityFrameworkSetup UseMySql<T>(
            this EntityFrameworkSetup setup,
            MySqlServerVersion        serverVersion)
            where T : DbContext
        {
            if (serverVersion == null)
                throw new ArgumentNullException(nameof(serverVersion));
            return setup.DbContext<UseMySql<T>>(s => s.TryAddSingleton(serverVersion));
        }

        public static EntityFrameworkSetup UseMySql<T, TC>(
            this EntityFrameworkSetup setup,
            MySqlServerVersion        serverVersion)
            where T  : DbContext
            where TC : IExtension<UseMySql<T>>
        {
            if (serverVersion == null)
                throw new ArgumentNullException(nameof(serverVersion));
            return setup.DbContext<UseMySql<T>, TC>(s => s.TryAddSingleton(serverVersion));
        }

        public static EntityFrameworkSetup UseMySql<T>(
            this EntityFrameworkSetup            setup,
            MySqlServerVersion                   serverVersion,
            Action<MySqlDbContextOptionsBuilder> configure)
            where T : DbContext
        {
            if (serverVersion == null)
                throw new ArgumentNullException(nameof(serverVersion));
            return configure != null
                 ? setup.DbContext<UseMySql<T>>(services =>
                 {
                     services.TryAddSingleton(serverVersion);
                     services.AddSingleton(new UseMySql<T>.ActionConfiguration(configure));
                 })
                 : setup.DbContext<UseMySql<T>>();
        }
        
        public static EntityFrameworkSetup UseMySql(
            this EntityFrameworkSetup setup,
            MySqlServerVersion        serverVersion,
            Type                      dbContextConfiguration = null)
        {
            if (serverVersion == null)
                throw new ArgumentNullException(nameof(serverVersion));
            return setup.DbContext(typeof(UseMySql<>), dbContextConfiguration,
                s => s.TryAddSingleton(serverVersion));
        }
        
        public static EntityFrameworkSetup UseMySql(
            this EntityFrameworkSetup            setup,
            MySqlServerVersion                   serverVersion,
            Action<MySqlDbContextOptionsBuilder> configure)
        {
            if (serverVersion == null)
                throw new ArgumentNullException(nameof(serverVersion));
            return setup.DbContext(typeof(UseMySql<>), action: services =>
            {
                services.TryAddSingleton(serverVersion);
                services.AddSingleton(new DefaultActionConfigurationProvider(configure));
            });
        }

        [Unmanaged]
        private class DefaultActionConfigurationProvider : Handler
        {
            private readonly Action<MySqlDbContextOptionsBuilder> _configure;

            public DefaultActionConfigurationProvider(
                Action<MySqlDbContextOptionsBuilder> configure)
            {
                _configure = configure;
            }

            [Provides, Singleton]
            public UseMySql<T>.Configuration Get<T>()
                where T : DbContext
            {
                return (UseMySql<T>.Configuration) Activator.CreateInstance(
                    typeof(UseMySql<T>.ActionConfiguration), _configure);
            }
        }
    }
}
