namespace Miruken.EntityFramework.SqlServer.Tests
{
    using EntityFramework.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TransactionTests : TransactionScenario
    {
        public TransactionTests() : base(new SqlServerSetup())
        {
        }
        
        protected override void Setup(EntityFrameworkSetup setup)
        {
            setup.DbContext(typeof(UseSqlServer<>));
        }
    }
}
