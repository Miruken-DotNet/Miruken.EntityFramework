namespace Miruken.EntityFramework
{
    using System;
    using Microsoft.EntityFrameworkCore;

    public class AsyncCountQuery<T> : AsyncScalarQuery<int>
    {
        public AsyncCountQuery(Query<T> query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            ContextQuery = c => query.ContextQuery(c).CountAsync();
        }
    }
}
