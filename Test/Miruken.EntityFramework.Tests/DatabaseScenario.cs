namespace Miruken.EntityFramework.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Callback;
    using Context;
    using Domain;
    using Log;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Register;

    public abstract class DatabaseScenario
    {
        private SportsContext _dbContext;
        
        protected DatabaseScenario(DatabaseSetup databaseSetup)
        {
            DatabaseSetup = databaseSetup
                ?? throw new ArgumentNullException(nameof(databaseSetup));
        }
      
        protected Context Context;

        protected DatabaseSetup DatabaseSetup { get; }
        
        protected abstract void Setup(EntityFrameworkSetup setup);
        
        protected virtual IEnumerable<Registration.SourceSelector> GetSources()
        {
            var domain = typeof(DatabaseScenario).Assembly;
            yield return source => source.FromAssemblies(domain);

            var test = GetType().Assembly;
            if (test != domain)
                yield return source => source.FromAssemblies(test);
        }
        
        [TestInitialize]
        public async Task TestInitialize()
        {
            var services             = new ServiceCollection();
            var configurationBuilder = new ConfigurationBuilder();

            await DatabaseSetup.Setup(configurationBuilder, services);

            var configuration = configurationBuilder.Build();
            
            Context = services
                .AddLogging()
                .AddSingleton(configuration)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(GetSources().ToArray())
                        .WithEntityFrameworkCore(Setup)
                        .WithLogging();
                }).Build();
            
            _dbContext = Context.Create<SportsContext>();
            
            await _dbContext.Database.EnsureCreatedAsync();
        }
        
        [TestCleanup]
        public async Task TestCleanup()
        {
            try
            {
                await using (_dbContext)
                {
                    if (_dbContext != null)
                        await _dbContext.Database.EnsureDeletedAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            try
            {
                await DatabaseSetup.DisposeAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Context?.End();
        }
    }
}
