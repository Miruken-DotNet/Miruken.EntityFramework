namespace Miruken.EntityFramework.Sqlite.Tests
{
    using EntityFramework.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sqlite;

    [TestClass]
    public class UnitOfWorkSqliteScenario : UnitOfWorkScenario
    {
        public UnitOfWorkSqliteScenario() : base(new SqliteSetup())
        {
        }
        
        protected override void Setup(EntityFrameworkSetup setup)
        {
            setup.UseSqlite();
        }
    }
}
