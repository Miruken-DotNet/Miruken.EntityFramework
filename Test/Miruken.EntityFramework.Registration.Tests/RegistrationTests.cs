// ReSharper disable All
namespace Miruken.EntityFramework.Registration.Tests
{
    using System;
    using System.Collections.Generic;
    using Callback;
    using EntityFramework.Tests;
    using EntityFramework.Tests.Domain;
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
        private IConfiguration _configuration;

        [TestInitialize]
        public void TestInitialize()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ConnectionStrings:SportsContext"] =
                        $"Data Source = sports_db_{Guid.NewGuid()}",
                    ["ConnectionStrings:CustomerContext"] =
                        $"Server=(LocalDB)\\MSSQLLocalDB;Database=sports_db_{Guid.NewGuid()};Trusted_Connection=True;MultipleActiveResultSets=true",
                });

            _configuration = configurationBuilder.Build();
        }

        [TestMethod]
        public void Should_Configure_DbContext()
        {
            var context = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(
                            sources => sources.FromAssemblyOf<RegistrationTests>(),
                            sources => sources.FromAssemblyOf<DatabaseScenario>())
                        .WithEntityFrameworkCore(setup => setup
                            .DbContext<UseSqlite<SportsContext>>()
                        )
                        .WithLogging();
                }).Build();

            using var sportsContext = context.Create<SportsContext>();
            Assert.IsNotNull(sportsContext);
        }

        [TestMethod]
        public void Should_Customize_DbContext()
        {
            var context = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(
                            sources => sources.FromAssemblyOf<RegistrationTests>(),
                            sources => sources.FromAssemblyOf<DatabaseScenario>())
                        .WithEntityFrameworkCore(setup => setup
                            .DbContext<UseSqlite<SportsContext>, ConfigureSqlite<SportsContext>>()
                        )
                        .WithLogging();
                }).Build();

            using var sportsContext = context.Create<SportsContext>();
            Assert.IsNotNull(sportsContext);
            Assert.IsTrue(ConfigureSqlite<SportsContext>.Configured);
        }

        [TestMethod]
        public void Should_Configure_DbContext_Fluently()
        {
            var context = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(
                            sources => sources.FromAssemblyOf<RegistrationTests>(),
                            sources => sources.FromAssemblyOf<DatabaseScenario>())
                        .WithEntityFrameworkCore(setup => setup
                            .UseSqlServer<SportsContext>()
                        )
                        .WithLogging();
                }).Build();

            using var sportsContext = context.Create<SportsContext>();
            Assert.IsNotNull(sportsContext);
        }

        [TestMethod]
        public void Should_Customize_DbContext_Fluently()
        {
            var customized = false;
            var context = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(
                            sources => sources.FromAssemblyOf<RegistrationTests>(),
                            sources => sources.FromAssemblyOf<DatabaseScenario>())
                        .WithEntityFrameworkCore(setup => setup
                            .UseSqlServer<SportsContext>(_ => customized = true)
                        )
                        .WithLogging();
                }).Build();

            using var sportsContext = context.Create<SportsContext>();
            Assert.IsNotNull(sportsContext);
            Assert.IsTrue(customized);
        }

        [TestMethod]
        public void Should_Allow_Default_Specification()
        {
            var context = new ServiceCollection()
                .AddSingleton(_configuration)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(
                            sources => sources.FromAssemblyOf<RegistrationTests>(),
                            sources => sources.FromAssemblyOf<DatabaseScenario>())
                        .WithEntityFrameworkCore(setup => setup
                            .UseSqlite<SportsContext, ConfigureSqlite<SportsContext>>()
                            .DbContext(typeof(UseSqlServer<>), typeof(ConfigureSqlServer<>))
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

        [TestMethod,
         ExpectedException(typeof(InvalidOperationException))]
        public void Should_Fail_If_DbContext_Already_Specified()
        {
            new ServiceCollection()
                .AddSingleton(_configuration)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(
                            sources => sources.FromAssemblyOf<RegistrationTests>(),
                            sources => sources.FromAssemblyOf<DatabaseScenario>())
                        .WithEntityFrameworkCore(setup => setup
                            .UseSqlite<SportsContext>()
                            .UseSqlServer<SportsContext>()
                        )
                        .WithLogging();
                }).Build();
        }

        [TestMethod,
         ExpectedException(typeof(InvalidOperationException))]
        public void Should_Fail_If_Default_DbContext_Already_Specified()
        {
            new ServiceCollection()
                .AddSingleton(_configuration)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(
                            sources => sources.FromAssemblyOf<RegistrationTests>(),
                            sources => sources.FromAssemblyOf<DatabaseScenario>())
                        .WithEntityFrameworkCore(setup => setup
                            .DbContext(typeof(UseSqlServer<>))
                            .DbContext(typeof(UseSqlite<>))
                        )
                        .WithLogging();
                }).Build();
        }

        [TestMethod,
         ExpectedException(typeof(ArgumentException))]
        public void Should_Fail_If_DbContext_Provider_Does_Not_Include_DbContext()
        {
            new ServiceCollection()
                .AddSingleton(_configuration)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(
                            sources => sources.FromAssemblyOf<RegistrationTests>(),
                            sources => sources.FromAssemblyOf<DatabaseScenario>())
                        .WithEntityFrameworkCore(setup => setup
                            .DbContext<DbContextOptions>()
                        )
                        .WithLogging();
                }).Build();
        }

        [TestMethod,
         ExpectedException(typeof(ArgumentException))]
        public void Should_Fail_If_DbContext_Configuration_Mismatch()
        {
            new ServiceCollection()
                .AddSingleton(_configuration)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(
                            sources => sources.FromAssemblyOf<RegistrationTests>(),
                            sources => sources.FromAssemblyOf<DatabaseScenario>())
                        .WithEntityFrameworkCore(setup => setup
                            .DbContext(typeof(UseSqlite<SportsContext>), typeof(ConfigureSqlServer<SportsContext>))
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

        public class ConfigureSqlServer<T> : UseSqlServer<T>.Configuration
            where T : DbContext
        {
            public static  bool Configured { get; set; }
            
            public override void Apply(SqlServerDbContextOptionsBuilder builder)
            {
                Configured = true;
            }
        }

        public class ConfigureSqlite<T> : UseSqlite<T>.Configuration
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
