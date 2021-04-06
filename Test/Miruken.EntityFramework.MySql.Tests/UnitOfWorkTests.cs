// ReSharper disable InconsistentNaming
namespace Miruken.EntityFramework.MySql.Tests
{
    using System;
    using EntityFramework.Tests;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnitOfWorkMySqlScenario : UnitOfWorkScenario
    {
        public UnitOfWorkMySqlScenario() : base(new MySqlSetup())
        {
        }
        
        protected override void Setup(EntityFrameworkSetup setup)
        {
            setup.UseMySql(new MySqlServerVersion(new Version(8, 0, 19)));
        }
    }
}
