namespace Miruken.EntityFramework.Tests.Api
{
    using Miruken.Api;

    public class CreateLeague : IRequest<LeagueResult>
    {
        public TeamData[] Teams { get; set; }
    }

    public class LeagueResult
    {
        public TeamData[] Teams { get; set; }
    }
}
