namespace Miruken.EntityFramework.MySql
{
    using System;
    using Callback;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;

    public static class EntityFrameworkSetupExtensions
    {
        public static EntityFrameworkSetup UseMySql<T>(
            this EntityFrameworkSetup setup)
            where T : DbContext
        {
            return setup.DbContext<UseMySql<T>>();
        }

        public static EntityFrameworkSetup UseMySql<T, TC>(
            this EntityFrameworkSetup setup)
            where T  : DbContext
            where TC : IExtension<UseMySql<T>>
        {
            return setup.DbContext<UseMySql<T>, TC>();
        }

        public static EntityFrameworkSetup UseMySql<T>(
            this EntityFrameworkSetup setup,
            Action<MySqlDbContextOptionsBuilder> configure)
            where T : DbContext
        {
            return configure != null
                 ? setup.DbContext<UseMySql<T>>(services => services
                     .AddSingleton(new UseMySql<T>.ActionConfiguration(configure)))
                 : setup.DbContext<UseMySql<T>>();
        }
        
        public static EntityFrameworkSetup UseMySql(
            this EntityFrameworkSetup setup,
            Type dbContextConfiguration = null)
        {
            return setup.DbContext(typeof(UseMySql<>), dbContextConfiguration);
        }
        
        public static EntityFrameworkSetup UseMySql(
            this EntityFrameworkSetup setup,
            Action<MySqlDbContextOptionsBuilder> configure)
        {
            return setup.DbContext(typeof(UseMySql<>), action: services =>
                services.AddSingleton(new DefaultActionConfigurationProvider(configure)));
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
