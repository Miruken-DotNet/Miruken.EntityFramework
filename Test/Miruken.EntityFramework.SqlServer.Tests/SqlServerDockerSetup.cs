// ReSharper disable InconsistentNaming
namespace Miruken.EntityFramework.SqlServer.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Docker.DotNet.Models;
    using EntityFramework.Tests;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    public class SqlServerDockerSetup : DockerDatabaseSetup
    {
        private const string Database = "miruken";
        private const string Password = "I@mJustT3st1ing";
        private const int    Port     = 1433;
        
        public SqlServerDockerSetup() : base("mcr.microsoft.com/mssql/server", "2019-latest", Port)
        {
        }

        protected override TimeSpan TimeOut => TimeSpan.FromSeconds(120);
        
        protected override CreateContainerParameters ConfigureContainer(
            ConfigurationBuilder configuration, int externalPort)
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:SportsContext"] = BuildConnectionString(externalPort, Database) 
            });

            return new CreateContainerParameters
            {
                Env  = new []
                {
                    "ACCEPT_EULA=Y",
                    $"SA_PASSWORD={Password}"
                }
            };
        }

        protected override async Task<bool> TestReady(int externalPort)
        {
            try
            {
                await using var connection = new SqlConnection(BuildConnectionString(externalPort, "master"));
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string BuildConnectionString(int externalPort, string database) =>
            $"Server=127.0.0.1,{externalPort};Database={database};User Id=sa;Password={Password};";
    }
}