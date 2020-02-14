namespace Miruken.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class SqlServerOptions<T> : DbContextOptions<T>
        where T : DbContext
    {
        public SqlServerOptions(IConfiguration configuration)
            : base(GetExtensions(configuration, null))
        {
        }

        public SqlServerOptions(IConfiguration configuration, ILoggerFactory loggerFactory)
            : base(GetExtensions(configuration, loggerFactory))
        {
        }

        private static IReadOnlyDictionary<Type, IDbContextOptionsExtension> 
            GetExtensions(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            var name             = typeof(T).Name;
            var connectionString = configuration.GetConnectionString(name) 
                ?? configuration.GetConnectionString(name.Replace("Context", ""))
                ?? throw new InvalidOperationException(
                       $"ConnectionString for '{name}' not found");

            var builder = new DbContextOptionsBuilder()
                .UseSqlServer(connectionString);

            if (loggerFactory != null)
                builder.UseLoggerFactory(loggerFactory);

            return builder.Options.Extensions.ToDictionary(p => p.GetType(), p => p);
        }
    }
}
