namespace Miruken.EntityFramework.Tests
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SqlServer;

    [TestClass]
    public class UnitOfWorkSqlServerTests : UnitOfWorkTests
    {
        protected override bool SupportsNestedTransactions => true;

        protected override Type DbContextOptionsType => typeof(SqlServerOptions<>);

        protected override DbContextOptions<SportsContext> GetDbContextOptions() =>
            new DbContextOptionsBuilder<SportsContext>()
                .UseSqlServer(
                    $"Server=(LocalDB)\\MSSQLLocalDB;Database=sports_db_{Guid.NewGuid()};Trusted_Connection=True;MultipleActiveResultSets=true")
                .Options;
    }
}
