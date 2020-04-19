// ReSharper disable InconsistentNaming
namespace Miruken.EntityFramework.PostgresSQL.Tests
{
    using EntityFramework.Tests;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnitOfWorkPostgresSQLScenario : UnitOfWorkScenario
    {
        public UnitOfWorkPostgresSQLScenario() : base(new PostgresSQLSetup())
        {
        }
        
        protected override void Setup(EntityFrameworkSetup setup)
        {
            setup.UsePostgresSQL();
        }
    }
}
