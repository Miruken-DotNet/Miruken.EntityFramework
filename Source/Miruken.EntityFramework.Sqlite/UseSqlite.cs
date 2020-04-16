namespace Miruken.EntityFramework.Sqlite
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class UseSqlite<T> : DbContextOptions<T>
        where T : DbContext
    {
        public UseSqlite(
                       IConfiguration configuration,
            [Optional] ILoggerFactory loggerFactory,
            [Optional] Configuration  configure)
            : base(configuration.
                CreateDbContextExtensions<T, SqliteDbContextOptionsBuilder>(
                    Setup, loggerFactory, configure != null ? configure.Apply
                        : (Action<SqliteDbContextOptionsBuilder>)null))
        {
        }

        private static void Setup(
            DbContextOptionsBuilder               builder, 
            IConfiguration                        configuration,
            string                                connectionString,
            Action<SqliteDbContextOptionsBuilder> configure = null)
        {
            if (configure != null)
                builder.UseSqlite(connectionString, configure);
            else
                builder.UseSqlite(connectionString);
        }

        public abstract class Configuration : IExtension<UseSqlite<T>>
        {
            public abstract void Apply(SqliteDbContextOptionsBuilder builder);
        }
    }
}
