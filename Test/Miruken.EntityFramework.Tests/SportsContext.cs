namespace Miruken.EntityFramework.Tests
{
    using Callback;
    using Domain;
    using Microsoft.EntityFrameworkCore;

    public interface ISportsContext : IDbContext { }

    public class SportsContext : DbContext, ISportsContext
    {
        [Creates]
        public SportsContext(DbContextOptions<SportsContext> options)
            : base(options)
        {
        }

        public DbSet<Team>   Teams  { get; }
        public DbSet<Person> People { get; }
    }
}
