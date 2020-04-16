namespace Miruken.EntityFramework.MySql
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class MySqlOptions<T> : DbContextOptions<T>
        where T : DbContext
    {
        public MySqlOptions(
            IConfiguration            configuration,
            [Optional] ILoggerFactory loggerFactory,
            [Optional] Options        options)
            : base(configuration.
                CreateDbContextExtensions<T, MySqlDbContextOptionsBuilder>(
                    UseSqlServer, loggerFactory,
                    options != null ? options.Configure :
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


        public abstract class Options : IExtension<MySqlOptions<T>>
        {
            public abstract void Configure(MySqlDbContextOptionsBuilder builder);
        }
    }
}
