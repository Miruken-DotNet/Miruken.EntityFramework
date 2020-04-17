namespace Miruken.EntityFramework.Sqlite.Tests
{
    using System;
    using System.Collections.Generic;
    using EntityFramework.Tests;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sqlite;

    [TestClass]
    public class TransactionTests : TransactionScenario
    {
        protected override void Setup(EntityFrameworkSetup setup)
        {
            setup.DbContext(typeof(UseSqlite<>));
        }

        protected override void Configure(
            ConfigurationBuilder configuration,
            IServiceCollection    services)
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:SportsContext"] = $"Data Source = sports_db_{Guid.NewGuid()}",
            });
        }
    }
}
