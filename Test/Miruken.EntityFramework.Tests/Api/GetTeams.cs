namespace Miruken.EntityFramework.Tests.Api
{
    using System;
    using Miruken.Api;

    public class GetTeams : IRequest<TeamResult>
    {
        public GetTeams()
            : this(Array.Empty<int>())
        {
        }

        public GetTeams(params int[] ids)
        {
            Ids = ids;
        }

        public int[] Ids            { get; set; }
        public bool  IncludePlayers { get; set; }
    }

    public class TeamResult
    {
        public TeamData[] Teams { get; set; }
    }
}
