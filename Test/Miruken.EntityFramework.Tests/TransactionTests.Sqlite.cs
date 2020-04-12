namespace Miruken.EntityFramework.Tests
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sqlite;

    [TestClass]
    public class TransactionSqliteTests : TransactionTests
    {
        protected override Type DbContextOptionsType => typeof(SqliteOptions<>);

        protected override DbContextOptions<SportsContext> GetDbContextOptions() =>
            new DbContextOptionsBuilder<SportsContext>()
                .UseSqlite($"Data Source = sports_db_{Guid.NewGuid()}")
                .Options;
    }
}
