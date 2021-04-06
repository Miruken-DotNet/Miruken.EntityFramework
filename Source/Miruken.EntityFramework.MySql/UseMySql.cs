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
            MySqlServerVersion        serverVersion,
            [Optional] ILoggerFactory loggerFactory,
            [Optional] Configuration  configure)
            : base(configuration.ApplyOptions<T, MySqlDbContextOptionsBuilder>(
                (b, c, cs, o) => UseSqlServer(b, c, cs, serverVersion, o),
                loggerFactory, configure))
        {
        }

        private static void UseSqlServer(
            DbContextOptionsBuilder              builder,
            IConfiguration                       configuration,
            string                               connectionString,
            MySqlServerVersion                   serverVersion,
            Action<MySqlDbContextOptionsBuilder> options = null)
        {
            builder.UseMySql(connectionString, serverVersion, options);
        }

        public abstract class Configuration : IExtension<UseMySql<T>>
        {
            public abstract void Apply(MySqlDbContextOptionsBuilder builder);
            
            public static implicit operator Action<MySqlDbContextOptionsBuilder>(
                Configuration c) => c != null ? c.Apply
                    : (Action<MySqlDbContextOptionsBuilder>) null;
        }
        
        public class ActionConfiguration : Configuration
        {
            private readonly Action<MySqlDbContextOptionsBuilder> _action;

            public ActionConfiguration(Action<MySqlDbContextOptionsBuilder> action)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }
            
            public override void Apply(MySqlDbContextOptionsBuilder builder) => _action(builder);
        }
    }
}
