namespace Miruken.EntityFramework.Tests
{
    using System;
    using Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Register;
    using ServiceCollection = Register.ServiceCollection;

    public abstract class DatabaseScenario
    {
        private SportsContext _context;

        protected Context Context;
        protected DbContextOptions<SportsContext> Options;

        [TestInitialize]
        public void TestInitialize()
        {
            Options = GetDbContextOptions();
            _context = new SportsContext(Options);
            _context.Database.EnsureCreated();

            Context = new ServiceCollection()
                .AddSingleton(Options)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(sources => sources.FromAssemblyOf<UnitOfWorkTests>())
                        .WithEntityFrameworkCore(DbContextOptionsType);
                }).Build();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            using (_context)
            {
                _context?.Database.EnsureDeleted();
            }

            Context?.End();
        }

        protected abstract Type DbContextOptionsType { get; }

        protected abstract DbContextOptions<SportsContext> GetDbContextOptions();
    }
}
