namespace Miruken.EntityFramework.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SqlServer;

    [TestClass]
    public class UnitOfWorkSqlServerTests : UnitOfWorkTests
    {
        protected override bool SupportsNestedTransactions => true;

        protected override void Setup(EntityFrameworkOptions options)
        {
            options.UseDefaultOptions(typeof(SqlServerOptions<>));
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
