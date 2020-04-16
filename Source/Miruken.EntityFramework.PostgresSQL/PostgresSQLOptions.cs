namespace Miruken.EntityFramework.PostgresSQL
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

    public class PostgresSQLOptions<T> : DbContextOptions<T>
        where T : DbContext
    {
        public PostgresSQLOptions(
                       IConfiguration configuration,
            [Optional] ILoggerFactory loggerFactory,
            [Optional] Options        options)
            : base(configuration.
                CreateDbContextExtensions<T, NpgsqlDbContextOptionsBuilder>(
                    UsePostgresSQL, loggerFactory,
                    options != null ? options.Configure :
                        (Action<NpgsqlDbContextOptionsBuilder>)null))
        {
        }

        private static void UsePostgresSQL(
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

        public abstract class Options : IExtension<PostgresSQLOptions<T>>
        {
            public abstract void Configure(NpgsqlDbContextOptionsBuilder builder);
        }
    }
}
