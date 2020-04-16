namespace Miruken.EntityFramework.SqlServer
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class UseSqlServer<T> : DbContextOptions<T>
        where T : DbContext
    {
        public UseSqlServer(
                       IConfiguration configuration,
            [Optional] ILoggerFactory loggerFactory,
            [Optional] Configuration  configure)
            : base(configuration.
                CreateDbContextExtensions<T, SqlServerDbContextOptionsBuilder>(
                    Setup, loggerFactory, configure != null ? configure.Apply
                        : (Action<SqlServerDbContextOptionsBuilder>)null))
        {
        }

        private static void Setup(
            DbContextOptionsBuilder                  builder,
            IConfiguration                           configuration,
            string                                   connectionString,
            Action<SqlServerDbContextOptionsBuilder> configure = null)
        {
            if (configure != null)
                builder.UseSqlServer(connectionString, configure);
            else
                builder.UseSqlServer(connectionString);
        }

        public abstract class Configuration : IExtension<UseSqlServer<T>>
        {
            public abstract void Apply(SqlServerDbContextOptionsBuilder builder);
        }
    }
}
