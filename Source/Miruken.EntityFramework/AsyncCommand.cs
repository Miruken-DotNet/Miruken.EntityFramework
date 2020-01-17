namespace Miruken.EntityFramework
{
    using System;
    using System.Threading.Tasks;

    public class AsyncCommand
    {
        protected Func<IDbContext, Task<int>> ContextQuery { get; set; }

        public virtual Task<int> ExecuteAsync(IDbContext context)
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
