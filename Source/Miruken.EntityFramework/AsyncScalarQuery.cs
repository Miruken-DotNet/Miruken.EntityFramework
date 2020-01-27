namespace Miruken.EntityFramework
{
    using System;
    using System.Threading.Tasks;

    public class AsyncScalarQuery<T>
    {
        public Func<IDbContext, Task<T>> ContextQuery { get; protected set; }

        public virtual Task<T> ExecuteAsync(IDbContext context)
        {
            EnsureContextQuery(context);

            return ContextQuery(context);
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
