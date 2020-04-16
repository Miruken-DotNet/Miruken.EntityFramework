namespace Miruken.EntityFramework.MySql
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class UseMySql<T> : DbContextOptions<T>
        where T : DbContext
    {
        public UseMySql(
            IConfiguration            configuration,
            [Optional] ILoggerFactory loggerFactory,
            [Optional] Configuration  configure)
            : base(configuration.
                CreateDbContextExtensions<T, MySqlDbContextOptionsBuilder>(
                    UseSqlServer, loggerFactory,
                    configure != null ? configure.Apply :
                        (Action<MySqlDbContextOptionsBuilder>)null))
        {
        }

        private static void UseSqlServer(
            DbContextOptionsBuilder              builder,
            IConfiguration                       configuration,
            string                               connectionString,
            Action<MySqlDbContextOptionsBuilder> options = null)
        {
            if (options != null)
                builder.UseMySql(connectionString, options);
            else
                builder.UseMySql(connectionString);
        }

        public abstract class Configuration : IExtension<UseMySql<T>>
        {
            public abstract void Apply(MySqlDbContextOptionsBuilder builder);
        }
    }
}
