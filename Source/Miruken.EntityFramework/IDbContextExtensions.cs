namespace Miruken.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public static class IDBContextExtensions
    {
        public static IQueryable<T> ExecuteStoredProcedure<T>(
            this IDbContext         context,
            string                  procedureName,
            params IDataParameter[] parameters) 
            where T : class
        {
            return context.Set<T>()
                .FromSqlRaw(Sql(procedureName, parameters), parameters);
        }

        public static int ExecuteStoredProcedure(
            this IDbContext         context, 
            string                  procedureName,
            params IDataParameter[] parameters)
        {
            return context.Database
                .ExecuteSqlRaw(Sql(procedureName, parameters), parameters);
        }

        public static Task<int> ExecuteStoredProcedureAsync(
            this IDbContext         context, 
            string                  procedureName,
            params IDataParameter[] parameters)
        {
            return context.Database
                .ExecuteSqlRawAsync(Sql(procedureName, parameters), parameters);
        }

        public static IReadOnlyDictionary<Type, IDbContextOptionsExtension>
            CreateDbContextExtensions<T, TB>(
            this IConfiguration configuration,
            Action<DbContextOptionsBuilder, IConfiguration, string, Action<TB>> configure,
            ILoggerFactory      loggerFactory = null,
            Action<TB>          options       = null)
            where T : DbContext
        {
            return CreateDbContextExtensions(configuration, typeof(T), configure, loggerFactory, options);
        }

        public static IReadOnlyDictionary<Type, IDbContextOptionsExtension>
            CreateDbContextExtensions<TB>(
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

        private static string Sql(
            string                  procedureName,
            params IDataParameter[] parameters)
        {
            return $"EXECUTE {procedureName} {StringifyIDataParameters(parameters)}";
        }

        private static string StringifyIDataParameters(IDataParameter[] dbParams)
        {
            if (dbParams == null || dbParams.Length == 0)
                return string.Empty;
            var sb = new StringBuilder();
            for (var i = 0; i < dbParams.Length; i++)
            {
                var param = dbParams[i];
                sb.Append(i == 0 ? " @" : ", @");
                sb.Append($"{param.ParameterName} = @{param.ParameterName}");
                if (param.Direction == ParameterDirection.InputOutput ||
                    param.Direction == ParameterDirection.Output)
                {
                    sb.Append(" OUTPUT");
                }
            }
            return sb.ToString();
        }
    }
}
