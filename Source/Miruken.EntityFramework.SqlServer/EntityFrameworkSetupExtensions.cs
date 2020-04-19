namespace Miruken.EntityFramework.SqlServer
{
    using System;
    using Callback;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;

    public static class EntityFrameworkSetupExtensions
    {
        public static EntityFrameworkSetup UseSqlServer<T>(
            this EntityFrameworkSetup setup)
            where T : DbContext
        {
            return setup.DbContext<UseSqlServer<T>>();
        }

        public static EntityFrameworkSetup UseSqlServer<T, TC>(
            this EntityFrameworkSetup setup)
            where T  : DbContext
            where TC : IExtension<UseSqlServer<T>>
        {
            return setup.DbContext<UseSqlServer<T>, TC>();
        }

        public static EntityFrameworkSetup UseSqlServer<T>(
            this EntityFrameworkSetup setup,
            Action<SqlServerDbContextOptionsBuilder> configure)
            where T : DbContext
        {
            return configure != null
                 ? setup.DbContext<UseSqlServer<T>>(services => services
                     .AddSingleton(new UseSqlServer<T>.ActionConfiguration(configure)))
                 : setup.DbContext<UseSqlServer<T>>();
        }
        
        public static EntityFrameworkSetup UseSqlServer(
            this EntityFrameworkSetup setup,
            Type dbContextConfiguration = null)
        {
            return setup.DbContext(typeof(UseSqlServer<>), dbContextConfiguration);
        }
        
        public static EntityFrameworkSetup UseSqlServer(
            this EntityFrameworkSetup setup,
            Action<SqlServerDbContextOptionsBuilder> configure)
        {
            return setup.DbContext(typeof(UseSqlServer<>), action: services =>
                services.AddSingleton(new DefaultActionConfigurationProvider(configure)));
        }

        [Unmanaged]
        private class DefaultActionConfigurationProvider : Handler
        {
            private readonly Action<SqlServerDbContextOptionsBuilder> _configure;

            public DefaultActionConfigurationProvider(
                Action<SqlServerDbContextOptionsBuilder> configure)
            {
                _configure = configure;
            }

            [Provides, Singleton]
            public UseSqlServer<T>.Configuration Get<T>()
                where T : DbContext
            {
                return (UseSqlServer<T>.Configuration) Activator.CreateInstance(
                    typeof(UseSqlServer<T>.ActionConfiguration), _configure);
            }
        }
    }
}
