namespace Miruken.EntityFramework.Tests
{
    using Callback;
    using Context;
    using Log;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Register;
    using ServiceCollection = Register.ServiceCollection;

    public abstract class DatabaseScenario
    {
        private SportsContext _context;

        protected Context Context;

        [TestInitialize]
        public void TestInitialize()
        {
            var services = new ServiceCollection();

            var configurationBuilder = new ConfigurationBuilder();
            Configure(configurationBuilder, services);

            Context = services
                .AddLogging()
                .AddSingleton(configurationBuilder.Build())
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(sources => sources.FromAssemblyOf<UnitOfWorkTests>())
                        .WithEntityFrameworkCore(Setup)
                        .WithLogging();
                }).Build();

            _context = Context.Create<SportsContext>();
            _context.Database.EnsureCreated();
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

        protected abstract void Setup(EntityFrameworkSetup setup);

        protected abstract void Configure(ConfigurationBuilder configuration, IServiceCollection   services);
    }
}
