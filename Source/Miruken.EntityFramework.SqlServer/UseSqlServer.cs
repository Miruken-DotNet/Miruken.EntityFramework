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
            : base(configuration.ApplyOptions<T, SqlServerDbContextOptionsBuilder>(
                    Setup, loggerFactory, configure))
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

            public static implicit operator Action<SqlServerDbContextOptionsBuilder>(
                Configuration c) => c != null ? c.Apply
                    : (Action<SqlServerDbContextOptionsBuilder>) null;
        }

        public class ActionConfiguration : Configuration
        {
            private readonly Action<SqlServerDbContextOptionsBuilder> _action;

            public ActionConfiguration(Action<SqlServerDbContextOptionsBuilder> action)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }
            
            public override void Apply(SqlServerDbContextOptionsBuilder builder) => _action(builder);
        }
    }
}
