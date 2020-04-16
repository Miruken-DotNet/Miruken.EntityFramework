namespace Miruken.EntityFramework.Tests
{
    using System;
    using System.Collections.Generic;
    using Callback;
    using Log;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Register;
    using Sqlite;
    using SqlServer;
    using ServiceCollection = Register.ServiceCollection;

    [TestClass]
    public class RegistrationTests
    {
        [TestMethod]
        public void Should_Override_DbContextOptions()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:SportsContext"] =
                        $"Data Source = sports_db_{Guid.NewGuid()}",
                    ["ConnectionStrings:CustomerContext"] =
                        $"Server=(LocalDB)\\MSSQLLocalDB;Database=sports_db_{Guid.NewGuid()};Trusted_Connection=True;MultipleActiveResultSets=true",
                });

            var context = new ServiceCollection()
                .AddSingleton(configurationBuilder.Build())
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(sources => sources.FromAssemblyOf<RegistrationTests>())
                        .WithEntityFrameworkCore(options => options
                            .UseDefaultOptions(typeof(SqlServerOptions<>), typeof(ConfigureSqlServer<>))
                            .UseDbContextOptions<SportsContext, SqliteOptions<SportsContext>, ConfigureSqlite<SportsContext>>()
                        )
                        .WithLogging();
                }).Build();

            using var sportsContext = context.Create<SportsContext>();
            Assert.IsNotNull(sportsContext);
            Assert.IsTrue(ConfigureSqlite<SportsContext>.Configured);

            using var customerContext = context.Create<CustomerContext>();
            Assert.IsNotNull(customerContext);
            Assert.IsTrue(ConfigureSqlServer<CustomerContext>.Configured);
        }

        public interface ICustomerContext : IDbContext { }

        public class CustomerContext : DbContext, ICustomerContext
        {
            [Creates]
            public CustomerContext(DbContextOptions<CustomerContext> options)
                : base(options)
            {
            }
        }

        public class ConfigureSqlServer<T> : SqlServerOptions<T>.Configure
            where T : DbContext
        {
            public static  bool Configured { get; set; }
            
            public override void Apply(SqlServerDbContextOptionsBuilder builder)
            {
                Configured = true;
            }
        }

        public class ConfigureSqlite<T> : SqliteOptions<T>.Configure
            where T : DbContext
        {
            public static bool Configured { get; set; }

            public override void Apply(SqliteDbContextOptionsBuilder builder)
            {
                Configured = true;
            }
        }
    }
}
