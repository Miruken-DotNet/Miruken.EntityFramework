namespace Miruken.EntityFramework
{
    using System;
    using System.Linq;

    public class CountQuery<T> : ScalarQuery<int>
    {
        public CountQuery(Query<T> query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            ContextQuery = c => query.ContextQuery(c).Count();
        }
    }
}
