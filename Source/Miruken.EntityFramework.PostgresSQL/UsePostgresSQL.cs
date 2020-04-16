namespace Miruken.EntityFramework.PostgresSQL
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

    public class UsePostgresSQL<T> : DbContextOptions<T>
        where T : DbContext
    {
        public UsePostgresSQL(
                       IConfiguration configuration,
            [Optional] ILoggerFactory loggerFactory,
            [Optional] Configuration  configure)
            : base(configuration.
                CreateDbContextExtensions<T, NpgsqlDbContextOptionsBuilder>(
                    Setup, loggerFactory, configure != null ? configure.Apply
                        : (Action<NpgsqlDbContextOptionsBuilder>)null))
        {
        }

        private static void Setup(
            DbContextOptionsBuilder               builder,
            IConfiguration                        configuration,
            string                                connectionString,
            Action<NpgsqlDbContextOptionsBuilder> options = null)
        {
            if (options != null)
                builder.UseNpgsql(connectionString, options);
            else
                builder.UseNpgsql(connectionString);
        }

        public abstract class Configuration : IExtension<UsePostgresSQL<T>>
        {
            public abstract void Apply(NpgsqlDbContextOptionsBuilder builder);
        }
    }
}
