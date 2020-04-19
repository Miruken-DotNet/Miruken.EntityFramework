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
            : base(configuration.ApplyOptions<T, SqliteDbContextOptionsBuilder>(
                    Setup, loggerFactory, configure))
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
            
            public static implicit operator Action<SqliteDbContextOptionsBuilder>(
                Configuration c) => c != null ? c.Apply
                    : (Action<SqliteDbContextOptionsBuilder>) null;
        }
        
        public class ActionConfiguration : Configuration
        {
            private readonly Action<SqliteDbContextOptionsBuilder> _action;

            public ActionConfiguration(Action<SqliteDbContextOptionsBuilder> action)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }
            
            public override void Apply(SqliteDbContextOptionsBuilder builder) => _action(builder);
        }
    }
}
