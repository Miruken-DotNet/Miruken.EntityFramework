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
                            .UseDefaultOptions(typeof(SqlServerOptions<>), typeof(ConfigureOptions<>))
                            .UseDbContextOptions<SportsContext, SqliteOptions<SportsContext>>()
                        )
                        .WithLogging();
                }).Build();

            using var sportsContext = context.Create<SportsContext>();
            Assert.IsNotNull(sportsContext);

            using var customerContext = context.Create<CustomerContext>();
            Assert.IsNotNull(customerContext);
            Assert.IsTrue(ConfigureOptions<CustomerContext>.Configured);
        }

        [TestMethod,
         ExpectedException(typeof(ArgumentException))]
        public void Should_Detect_Options_Configuration_Mismatch()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:SportsContext"] =
                        $"Data Source = sports_db_{Guid.NewGuid()}"
                });

            new ServiceCollection()
                .AddSingleton(configurationBuilder.Build())
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(sources => sources.FromAssemblyOf<RegistrationTests>())
                        .WithEntityFrameworkCore(options => options
                            .UseDbContextOptions<SportsContext, SqliteOptions<SportsContext>, ConfigureOptions<SportsContext>>()
                        )
                        .WithLogging();
                }).Build();
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

        public class ConfigureOptions<T> : SqlServerOptions<T>.Options
            where T : DbContext
        {
            public static  bool Configured { get; set; }
            
            public override void Configure(SqlServerDbContextOptionsBuilder builder)
            {
                Configured = true;
            }
        }
    }
}
