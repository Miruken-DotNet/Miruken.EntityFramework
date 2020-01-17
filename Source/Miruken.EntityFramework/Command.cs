namespace Miruken.EntityFramework
{
    using System;

    public class Command
    {
        protected Func<IDbContext, int> ContextQuery { get; set; }

        public virtual int Execute(IDbContext context)
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
