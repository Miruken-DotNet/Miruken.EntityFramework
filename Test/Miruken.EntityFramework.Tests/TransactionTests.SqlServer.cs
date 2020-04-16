namespace Miruken.EntityFramework.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SqlServer;

    [TestClass]
    public class TransactionSqlServerTests : TransactionTests
    {
        protected override void Setup(EntityFrameworkSetup setup)
        {
            setup.DbContext(typeof(UseSqlServer<>));
        }

        protected override void Configure(
            ConfigurationBuilder configuration,
            IServiceCollection   services)
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:SportsContext"] =
                    $"Server=(LocalDB)\\MSSQLLocalDB;Database=sports_db_{Guid.NewGuid()};Trusted_Connection=True;MultipleActiveResultSets=true",
            });
        }
    }
}
