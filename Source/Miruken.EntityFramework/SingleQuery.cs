namespace Miruken.EntityFramework
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public class SingleQuery<T>
    {
        public Func<IDbContext, IQueryable<T>> ContextQuery { get; protected set; }

        public virtual T Execute(IDbContext context)
        {
            EnsureContextQuery(context);

            return ContextQuery(context).SingleOrDefault();
        }

        public virtual Task<T> ExecuteAsync(IDbContext context)
        {
            EnsureContextQuery(context);

            return ContextQuery(context).SingleOrDefaultAsync();
        }

        private void EnsureContextQuery(IDbContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (ContextQuery == null)
            {
                throw new InvalidOperationException(
                    "The ContextQuery property has not been assigned");
            }
        }
    }
}
