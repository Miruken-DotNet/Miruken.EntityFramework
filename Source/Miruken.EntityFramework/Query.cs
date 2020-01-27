namespace Miruken.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public class Query<T>
    {
        public Func<IDbContext, IQueryable<T>> ContextQuery { get; protected set; }

        public virtual IEnumerable<T> Execute(IDbContext context)
        {
            EnsureContextQuery(context);

            return ContextQuery(context).ToList();
        }

        public virtual Task<IEnumerable<T>> ExecuteAsync(IDbContext context)
        {
            EnsureContextQuery(context);

            return ContextQuery(context).ToListAsync()
                .ContinueWith(t => (IEnumerable<T>)t.Result);
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
