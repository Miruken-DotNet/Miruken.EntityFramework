namespace Miruken.EntityFramework.Tests.Api
{
    using Miruken.Api;

    public class CreateTeam : IRequest<TeamData>
    {
        public TeamData Team { get; set; }
    }
}
