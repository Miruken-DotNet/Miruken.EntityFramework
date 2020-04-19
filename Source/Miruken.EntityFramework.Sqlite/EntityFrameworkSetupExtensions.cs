namespace Miruken.EntityFramework.Sqlite
{
    using System;
    using Callback;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;

    public static class EntityFrameworkSetupExtensions
    {
        public static EntityFrameworkSetup UseSqlite<T>(
            this EntityFrameworkSetup setup)
            where T : DbContext
        {
            return setup.DbContext<UseSqlite<T>>();
        }

        public static EntityFrameworkSetup UseSqlite<T, TC>(
            this EntityFrameworkSetup setup)
            where T  : DbContext
            where TC : IExtension<UseSqlite<T>>
        {
            return setup.DbContext<UseSqlite<T>, TC>();
        }

        public static EntityFrameworkSetup UseSqlite<T>(
            this EntityFrameworkSetup setup,
            Action<SqliteDbContextOptionsBuilder> configure)
            where T : DbContext
        {
            return configure != null
                 ? setup.DbContext<UseSqlite<T>>(services => services
                     .AddSingleton(new UseSqlite<T>.ActionConfiguration(configure)))
                 : setup.DbContext<UseSqlite<T>>();
        }
        
        public static EntityFrameworkSetup UseSqlite(
            this EntityFrameworkSetup setup,
            Type dbContextConfiguration = null)
        {
            return setup.DbContext(typeof(UseSqlite<>), dbContextConfiguration);
        }
        
        public static EntityFrameworkSetup UseSqlite(
            this EntityFrameworkSetup setup,
            Action<SqliteDbContextOptionsBuilder> configure)
        {
            return setup.DbContext(typeof(UseSqlite<>), action: services =>
                services.AddSingleton(new DefaultActionConfigurationProvider(configure)));
        }

        [Unmanaged]
        private class DefaultActionConfigurationProvider : Handler
        {
            private readonly Action<SqliteDbContextOptionsBuilder> _configure;

            public DefaultActionConfigurationProvider(
                Action<SqliteDbContextOptionsBuilder> configure)
            {
                _configure = configure;
            }

            [Provides, Singleton]
            public UseSqlite<T>.Configuration Get<T>()
                where T : DbContext
            {
                return (UseSqlite<T>.Configuration) Activator.CreateInstance(
                    typeof(UseSqlite<T>.ActionConfiguration), _configure);
            }
        }
    }
}
