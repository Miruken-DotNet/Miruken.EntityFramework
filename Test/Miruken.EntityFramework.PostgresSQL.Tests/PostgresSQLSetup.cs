// ReSharper disable InconsistentNaming
namespace Miruken.EntityFramework.PostgresSQL.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Docker.DotNet.Models;
    using EntityFramework.Tests;
    using Microsoft.Extensions.Configuration;
    using Npgsql;

    public class PostgresSQLSetup : DockerDatabaseSetup
    {
        private const string Database = "miruken";
        private const string UserName = "postgres";
        private const string Password = "password";
        private const int    Port     = 5432;
        
        public PostgresSQLSetup() : base("postgres", "alpine", Port)
        {
        }

        protected override CreateContainerParameters ConfigureContainer(
            ConfigurationBuilder configuration, int externalPort)
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:SportsContext"] = BuildConnectionString(externalPort) 
            });

            return new CreateContainerParameters
            {
                User = UserName,
                Env  = new []
                {
                    $"POSTGRES_PASSWORD={Password}",
                    $"POSTGRES_DB={Database}",
                    $"POSTGRES_USER={UserName}"
                }
            };
        }

        protected override async Task<bool> TestReady(int externalPort)
        {
            try
            {
                await using var connection = new NpgsqlConnection(BuildConnectionString(externalPort));
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string BuildConnectionString(int externalPort) =>
            $"User ID={UserName};Password={Password};Server=127.0.0.1;Port={externalPort};Database={Database};Integrated Security=true;Pooling=false;CommandTimeout=3000";
    }
}