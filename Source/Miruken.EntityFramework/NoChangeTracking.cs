namespace Miruken.EntityFramework
{
    using System;

    public sealed class NoChangeTracking : IDisposable
    {
        private readonly IDbContext _dbContext;
        private readonly bool? _initialTracking;

        public NoChangeTracking(IDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            var changeTracker = _dbContext.ChangeTracker;
            if (changeTracker != null)
            {
                _initialTracking = changeTracker.AutoDetectChangesEnabled;
                changeTracker.AutoDetectChangesEnabled = false;
            }
        }

        public void Dispose()
        {
            if (_initialTracking != null)
                _dbContext.ChangeTracker.AutoDetectChangesEnabled = _initialTracking.Value;
        }
    }

    public static class NoChangeTrackingExtensions
    {
        public static NoChangeTracking NoChangeTracking(this IDbContext dbContext)
        {
            return new NoChangeTracking(dbContext);
        }
    }
}
