namespace Miruken.EntityFramework.Sqlite
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class SqliteOptions<T> : DbContextOptions<T>
        where T : DbContext
    {
        public SqliteOptions(IConfiguration configuration)
            : base(configuration.CreateDbContextExtensions<T>(UseSqlite))
        {
        }

        public SqliteOptions(IConfiguration configuration, ILoggerFactory loggerFactory)
            : base(configuration.CreateDbContextExtensions<T>(UseSqlite, loggerFactory))
        {
        }

        private static void UseSqlite(
            DbContextOptionsBuilder builder, 
            IConfiguration          configuration,
            string                  connectionString)
        {
            builder.UseSqlite(connectionString);
        }
    }
}
