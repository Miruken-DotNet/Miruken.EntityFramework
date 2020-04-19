namespace Miruken.EntityFramework.PostgresSQL.Tests
{
    using EntityFramework.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TransactionTests : TransactionScenario
    {
        public TransactionTests() : base(new PostgresSQLSetup())
        {
        }
        
        protected override void Setup(EntityFrameworkSetup setup)
        {
            setup.UsePostgresSQL();
        }
    }
}
