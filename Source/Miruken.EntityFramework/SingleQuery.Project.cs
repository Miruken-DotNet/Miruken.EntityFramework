namespace Miruken.EntityFramework
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public class SingleQuery<T, P>
    {
        public Func<IDbContext, IQueryable<T>> ContextQuery { get; protected set; }
        public Func<T, P>                      Project      { get; protected set; }

        public virtual P Execute(IDbContext context)
        {
            EnsureContextQuery(context);

            var single = ContextQuery(context).SingleOrDefault();
            return single != null ? Project(single) : default;
        }

        public virtual Task<P> ExecuteAsync(IDbContext context)
        {
            EnsureContextQuery(context);

            return ContextQuery(context).SingleOrDefaultAsync()
                .ContinueWith(t => t.Result != null ? Project(t.Result) : default);
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
