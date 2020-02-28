namespace Miruken.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public class Query<T, P>
    {
        public Func<IDbContext, IQueryable<T>>      ContextQuery { get; protected set; }
        public Func<IEnumerable<T>, IEnumerable<P>> Project      { get; protected set; }

        public virtual IEnumerable<P> Execute(IDbContext context)
        {
            EnsureContextQuery(context);

            return Project(ContextQuery(context).ToList());
        }

        public virtual Task<IEnumerable<P>> ExecuteAsync(IDbContext context)
        {
            EnsureContextQuery(context);

            return ContextQuery(context).ToListAsync()
                .ContinueWith(t => Project(t.Result));
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

            if (Project == null)
            {
                throw new InvalidOperationException(
                    "The Project property has not been assigned");
            }
        }
    }
}
