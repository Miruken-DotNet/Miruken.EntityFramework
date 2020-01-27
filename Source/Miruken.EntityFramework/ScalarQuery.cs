namespace Miruken.EntityFramework
{
    using System;

    public class ScalarQuery<T>
    {
        public Func<IDbContext, T> ContextQuery { get; protected set; }

        public virtual T Execute(IDbContext context)
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
