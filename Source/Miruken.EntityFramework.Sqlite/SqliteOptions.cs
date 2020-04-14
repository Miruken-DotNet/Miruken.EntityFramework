namespace Miruken.EntityFramework.Sqlite
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class SqliteOptions<T> : DbContextOptions<T>
        where T : DbContext
    {
        public SqliteOptions(
                       IConfiguration configuration,
            [Optional] ILoggerFactory loggerFactory,
            [Optional] Options        options)
            : base(configuration.
                CreateDbContextExtensions<T, SqliteDbContextOptionsBuilder>(
                    UseSqlite, loggerFactory,
                    options != null ? options.Configure :
                        (Action<SqliteDbContextOptionsBuilder>)null))
        {
        }

        private static void UseSqlite(
            DbContextOptionsBuilder               builder, 
            IConfiguration                        configuration,
            string                                connectionString,
            Action<SqliteDbContextOptionsBuilder> options = null)
        {
            if (options != null)
                builder.UseSqlite(connectionString, options);
            else
                builder.UseSqlite(connectionString);
        }

        public abstract class Options
        {
            public abstract void Configure(SqliteDbContextOptionsBuilder builder);
        }
    }
}
