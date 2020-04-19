namespace Miruken.EntityFramework.MySql.Tests
{
    using EntityFramework.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TransactionTests : TransactionScenario
    {
        public TransactionTests() : base(new MySqlSetup())
        {
        }
        
        protected override void Setup(EntityFrameworkSetup setup)
        {
            setup.UseMySql();
        }
    }
}
