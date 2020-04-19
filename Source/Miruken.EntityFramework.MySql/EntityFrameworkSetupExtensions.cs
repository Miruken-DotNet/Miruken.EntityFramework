namespace Miruken.EntityFramework.MySql
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public static class EntityFrameworkSetupExtensions
    {
        public static EntityFrameworkSetup UseMySql(
            this EntityFrameworkSetup setup,
            Type dbContextConfiguration = null)
        {
            return setup.DbContext(typeof(UseMySql<>), dbContextConfiguration);
        }

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
                 ? setup.DbContext<UseMySql<T>, MySqlConfiguration<T>>(
                    new MySqlConfiguration<T>(configure))
                 : setup.DbContext<UseMySql<T>>();
        }

        private class MySqlConfiguration<T> : UseMySql<T>.Configuration
            where T : DbContext
        {
            private readonly Action<MySqlDbContextOptionsBuilder> _configure;

            public MySqlConfiguration(
                Action<MySqlDbContextOptionsBuilder> configure)
            {
                _configure = configure;
            }

            public override void Apply(MySqlDbContextOptionsBuilder builder)
            {
                _configure(builder);
            }
        }
    }
}
