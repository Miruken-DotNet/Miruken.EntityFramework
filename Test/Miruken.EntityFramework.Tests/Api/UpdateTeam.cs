namespace Miruken.EntityFramework.Tests.Api
{
    using Miruken.Api;

    public class UpdateTeam : IRequest<TeamData>
    {
        public TeamData Team { get; set; }
    }
}
