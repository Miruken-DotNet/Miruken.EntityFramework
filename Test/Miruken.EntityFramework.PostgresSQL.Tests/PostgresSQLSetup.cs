// ReSharper disable InconsistentNaming
namespace Miruken.EntityFramework.PostgresSQL.Tests
{
    using System.Collections.Generic;
    using Docker.DotNet.Models;
    using EntityFramework.Tests;
    using Microsoft.Extensions.Configuration;

    public class PostgresSQLSetup : DockerDatabaseSetup
    {
        private const string DatabaseName = "miruken";
        private const string UserName     = "postgres";
        private const string Password     = "password";
        private const int    Port         = 5432;
        
        public PostgresSQLSetup() : base("postgres", "alpine", Port)
        {
        }

        protected override CreateContainerParameters ConfigureContainer(
            ConfigurationBuilder configuration, int externalPort)
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:SportsContext"] =
                    $"User ID={UserName};Password={Password};Server=127.0.0.1;Port={externalPort};Database={DatabaseName};Integrated Security=true;Pooling=false;CommandTimeout=3000"
            });

            return new CreateContainerParameters
            {
                User = UserName,
                Env  = new []
                {
                    $"POSTGRES_PASSWORD={Password}",
                    $"POSTGRES_DB={DatabaseName}",
                    $"POSTGRES_USER={UserName}"
                }
            };
        }
    }
}