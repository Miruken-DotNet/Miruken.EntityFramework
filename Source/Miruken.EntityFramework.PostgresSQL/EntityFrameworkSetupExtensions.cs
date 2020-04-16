namespace Miruken.EntityFramework.PostgresSQL
{
    using System;
    using Microsoft.EntityFrameworkCore;
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
                 ? setup.DbContext<UsePostgresSQL<T>, PostgresSQLConfiguration<T>>(
                     new PostgresSQLConfiguration<T>(configure))
                 : setup.DbContext<UsePostgresSQL<T>>();
        }

        private class PostgresSQLConfiguration<T> : UsePostgresSQL<T>.Configuration
            where T : DbContext
        {
            private readonly Action<NpgsqlDbContextOptionsBuilder> _configure;

            public PostgresSQLConfiguration(
                Action<NpgsqlDbContextOptionsBuilder> configure)
            {
                _configure = configure;
            }

            public override void Apply(NpgsqlDbContextOptionsBuilder builder)
            {
                _configure(builder);
            }
        }
    }
}
