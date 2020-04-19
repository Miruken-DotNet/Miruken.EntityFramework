// ReSharper disable InconsistentNaming
namespace Miruken.EntityFramework.MySql.Tests
{
    using EntityFramework.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnitOfWorkPostgresSQLScenario : UnitOfWorkScenario
    {
        public UnitOfWorkPostgresSQLScenario() : base(new MySqlSetup())
        {
        }
        
        protected override void Setup(EntityFrameworkSetup setup)
        {
            setup.UseMySql();
        }
    }
}
