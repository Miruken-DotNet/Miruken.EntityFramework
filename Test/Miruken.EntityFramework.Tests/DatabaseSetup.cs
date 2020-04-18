namespace Miruken.EntityFramework.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public abstract class DatabaseSetup : IAsyncDisposable
    {
        public virtual bool SupportsNestedTransactions => true;
        
        public abstract ValueTask Setup(
            ConfigurationBuilder configuration,
            IServiceCollection   services);
        
        public virtual ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}