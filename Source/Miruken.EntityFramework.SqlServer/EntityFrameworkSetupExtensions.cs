namespace Miruken.EntityFramework.SqlServer
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public static class EntityFrameworkSetupExtensions
    {
        public static EntityFrameworkSetup UseSqlServer(
            this EntityFrameworkSetup setup,
            Type dbContextConfiguration = null)
        {
            return setup.DbContext(typeof(UseSqlServer<>), dbContextConfiguration);
        }
        
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
                 ? setup.DbContext<UseSqlServer<T>, SqlServerConfiguration<T>>(
                    new SqlServerConfiguration<T>(configure))
                 : setup.DbContext<UseSqlServer<T>>();
        }

        private class SqlServerConfiguration<T> : UseSqlServer<T>.Configuration
            where T : DbContext
        {
            private readonly Action<SqlServerDbContextOptionsBuilder> _configure;

            public SqlServerConfiguration(
                Action<SqlServerDbContextOptionsBuilder> configure)
            {
                _configure = configure;
            }

            public override void Apply(SqlServerDbContextOptionsBuilder builder)
            {
                _configure(builder);
            }
        }
    }
}
