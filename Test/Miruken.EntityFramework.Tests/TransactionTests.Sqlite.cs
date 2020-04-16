﻿namespace Miruken.EntityFramework.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sqlite;

    [TestClass]
    public class TransactionSqliteTests : TransactionTests
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