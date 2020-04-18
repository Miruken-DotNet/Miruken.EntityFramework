namespace Miruken.EntityFramework.Sqlite.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EntityFramework.Tests;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class SqliteSetup : DatabaseSetup
    {
        public override bool SupportsNestedTransactions => false;

        public override ValueTask Setup(
            ConfigurationBuilder configuration,
            IServiceCollection   services)
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:SportsContext"] = $"Data Source = sports_db_{Guid.NewGuid()}"           });
            
            return new ValueTask();
        }
    }
}