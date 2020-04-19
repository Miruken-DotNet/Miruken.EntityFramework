// ReSharper disable InconsistentNaming
namespace Miruken.EntityFramework.PostgresSQL
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

    public class UsePostgresSQL<T> : DbContextOptions<T>
        where T : DbContext
    {
        public UsePostgresSQL(
                       IConfiguration configuration,
            [Optional] ILoggerFactory loggerFactory,
            [Optional] Configuration  configure)
            : base(configuration.ApplyOptions<T, NpgsqlDbContextOptionsBuilder>(
                    Setup, loggerFactory, configure))
        {
        }

        private static void Setup(
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

        public abstract class Configuration : IExtension<UsePostgresSQL<T>>
        {
            public abstract void Apply(NpgsqlDbContextOptionsBuilder builder);
            
            public static implicit operator Action<NpgsqlDbContextOptionsBuilder>(
                Configuration c) => c != null ? c.Apply
                    : (Action<NpgsqlDbContextOptionsBuilder>) null;
        }
        
        public class ActionConfiguration : Configuration
        {
            private readonly Action<NpgsqlDbContextOptionsBuilder> _action;

            public ActionConfiguration(Action<NpgsqlDbContextOptionsBuilder> action)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }
            
            public override void Apply(NpgsqlDbContextOptionsBuilder builder) => _action(builder);
        }
    }
}
