// ReSharper disable InconsistentNaming
namespace Miruken.EntityFramework.MySql.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Docker.DotNet.Models;
    using EntityFramework.Tests;
    using Microsoft.Extensions.Configuration;
    using MySqlConnector;

    public class MySqlSetup : DockerDatabaseSetup
    {
        private const string Database = "miruken";
        private const string Password = "rootpwd123";
        private const int    Port     = 3306;
        
        public MySqlSetup() : base("mysql", "8.0.19", Port)
        {
        }

        protected override CreateContainerParameters ConfigureContainer(
            ConfigurationBuilder configuration, int externalPort)
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:SportsContext"] = BuildConnectionString(externalPort, Database) 
            });

            return new CreateContainerParameters
            {
                Env = new []
                {
                    $"MYSQL_ROOT_PASSWORD={Password}"
                }
            };
        }

        protected override async Task<bool> TestReady(int externalPort)
        {
            try
            {
                await using var connection = new MySqlConnection(BuildConnectionString(externalPort, "mysql"));
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string BuildConnectionString(int externalPort, string database) =>
            $"Server=127.0.0.1;Port={externalPort};Database={database};User=root;Password={Password};";
    }
}