namespace Miruken.EntityFramework.SqlServer.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EntityFramework.Tests;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class SqlServerLocalSetup : DatabaseSetup
    {
        public override ValueTask Setup(
            ConfigurationBuilder configuration,
            IServiceCollection   services)
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:SportsContext"] =
                    $"Server=(LocalDB)\\MSSQLLocalDB;Database=sports_db_{Guid.NewGuid()};Trusted_Connection=True;MultipleActiveResultSets=true",
            });
            
            return new ValueTask();
        }
    }
}