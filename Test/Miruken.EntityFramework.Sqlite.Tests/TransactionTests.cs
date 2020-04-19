namespace Miruken.EntityFramework.Sqlite.Tests
{
    using EntityFramework.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sqlite;

    [TestClass]
    public class TransactionTests : TransactionScenario
    {
        public TransactionTests() : base(new SqliteSetup())
        {
        }
        
        protected override void Setup(EntityFrameworkSetup setup)
        {
            setup.UseSqlite();
        }
    }
}
