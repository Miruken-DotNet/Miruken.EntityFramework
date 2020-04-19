namespace Miruken.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public static class DbContextOptionsExtensions
    {
        public static IReadOnlyDictionary<Type, IDbContextOptionsExtension>
            ApplyOptions<TB>(
                this IConfiguration configuration, 
                Type                dbContextType, 
                Action<DbContextOptionsBuilder, IConfiguration, string, Action<TB>> configure, 
                ILoggerFactory      loggerFactory = null,
                Action<TB>          options       = null)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            if (dbContextType == null)
                throw new ArgumentNullException(nameof(dbContextType));

            if (!typeof(DbContext).IsAssignableFrom(dbContextType))
                throw new ArgumentException($"Type {dbContextType.FullName} is not a DbContext");

            var name             = dbContextType.Name;
            var connectionString = configuration.GetConnectionString(name)
                                   ?? configuration.GetConnectionString(name.Replace("Context", ""))
                                   ?? throw new InvalidOperationException(
                                       $"ConnectionString for '{name}' not found");

            var builder = new DbContextOptionsBuilder();
            configure(builder, configuration, connectionString, options);

            if (loggerFactory != null)
                builder.UseLoggerFactory(loggerFactory);

            return builder.Options.Extensions.ToDictionary(p => p.GetType(), p => p);
        }

        public static IReadOnlyDictionary<Type, IDbContextOptionsExtension>
            ApplyOptions<T, TB>(
                this IConfiguration configuration,
                Action<DbContextOptionsBuilder, IConfiguration, string, Action<TB>> configure,
                ILoggerFactory      loggerFactory = null,
                Action<TB>          options       = null)
            where T : DbContext
        {
            return ApplyOptions(configuration, typeof(T), configure, loggerFactory, options);
        }
    }
}