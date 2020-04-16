namespace Miruken.EntityFramework.Sqlite
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

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
                 ? setup.DbContext<UseSqlite<T>, SqliteConfiguration<T>>(
                     new SqliteConfiguration<T>(configure))
                 : setup.DbContext<UseSqlite<T>>();
        }

        private class SqliteConfiguration<T> : UseSqlite<T>.Configuration
            where T : DbContext
        {
            private readonly Action<SqliteDbContextOptionsBuilder> _configure;

            public SqliteConfiguration(
                Action<SqliteDbContextOptionsBuilder> configure)
            {
                _configure = configure;
            }

            public override void Apply(SqliteDbContextOptionsBuilder builder)
            {
                _configure(builder);
            }
        }
    }
}
