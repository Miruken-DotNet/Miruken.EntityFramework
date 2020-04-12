namespace Miruken.EntityFramework.SqlServer
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class SqlServerOptions<T> : DbContextOptions<T>
        where T : DbContext
    {
        public SqlServerOptions(IConfiguration configuration)
            : base(configuration.CreateDbContextExtensions<T>(UseSqlServer))
        {
        }

        public SqlServerOptions(IConfiguration configuration, ILoggerFactory loggerFactory)
            : base(configuration.CreateDbContextExtensions<T>(UseSqlServer, loggerFactory))
        {
        }

        private static void UseSqlServer(
            DbContextOptionsBuilder builder,
            IConfiguration          configuration,
            string                  connectionString)
        {
            builder.UseSqlServer(connectionString);
        }
    }
}
