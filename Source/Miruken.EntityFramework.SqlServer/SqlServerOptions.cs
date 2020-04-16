namespace Miruken.EntityFramework.SqlServer
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class SqlServerOptions<T> : DbContextOptions<T>
        where T : DbContext
    {
        public SqlServerOptions(
                       IConfiguration configuration,
            [Optional] ILoggerFactory loggerFactory,
            [Optional] Configure      configure)
            : base(configuration.
                CreateDbContextExtensions<T, SqlServerDbContextOptionsBuilder>(
                    UseSqlServer, loggerFactory,
                    configure != null ? configure.Apply :
                        (Action<SqlServerDbContextOptionsBuilder>)null))
        {
        }

        private static void UseSqlServer(
            DbContextOptionsBuilder                  builder,
            IConfiguration                           configuration,
            string                                   connectionString,
            Action<SqlServerDbContextOptionsBuilder> options = null)
        {
            if (options != null)
                builder.UseSqlServer(connectionString, options);
            else
                builder.UseSqlServer(connectionString);
        }

        public abstract class Configure : IExtension<SqlServerOptions<T>>
        {
            public abstract void Apply(SqlServerDbContextOptionsBuilder builder);
        }
    }
}
