namespace Miruken.EntityFramework.SqlServer.Tests
{
    using EntityFramework.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SqlServer;

    [TestClass]
    public class UnitOfWorkTests : UnitOfWorkScenario
    {
        public UnitOfWorkTests() : base(new SqlServerSetup())
        {
        }
        
        protected override void Setup(EntityFrameworkSetup setup)
        {
            setup.DbContext(typeof(UseSqlServer<>));
        }
    }
}
