namespace Miruken.EntityFramework.Tests.Domain
{
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public abstract class QueryTeam
    {
        public class ById : Query<Team>
        {
            public ById(params int[] ids)
            {
                ContextQuery = c =>
                {
                    IQueryable<Team> query = c.Set<Team>()
                        .Include(x => x.Coach);

                    if (IncludePlayers)
                        query = query.Include(x => x.Players);

                    if (ids?.Length == 1)
                    {
                        var id = ids[0];
                        query = query.Where(x => x.Id == id);
                    }
                    else if (ids?.Length > 1)
                    {
                        query = query.Where(x => ids.Contains(x.Id));
                    }

                    return query;
                };
            }

            public bool IncludePlayers { get; set; }
        }
    }
}
