namespace Miruken.EntityFramework.Tests.Api
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Callback;
    using Domain;
    using Map;

    public class TeamHandler
    {
        [Handles, UnitOfWork]
        public async Task<TeamResult> Get(
            GetTeams       request,
            ISportsContext sports,
            IHandler       composer)
        {
            var Teams = (await new QueryTeam.ById(request.Ids)
                {
                    IncludePlayers = request.IncludePlayers
                }.ExecuteAsync(sports))
                 .Select(x => composer.Map<TeamData>(x))
                 .ToArray();

            return new TeamResult
            {
                Teams = Teams
            };
        }

        [Handles, UnitOfWork]
        public TeamData Create(
            CreateTeam                 request,
            UnitOfWork<ISportsContext> sports,
            IHandler                   composer)
        {
            var entity = composer.Map<Team>(request.Team);
            var data   = new TeamData();
            sports.Add(entity, () => data.Id = entity.Id);
            return data;
        }

        [Handles, UnitOfWork]
        public async Task<TeamData> Update(
            UpdateTeam     request,
            ISportsContext sports,
            IHandler       composer)
        {
            var entity = (await new QueryTeam.ById(request.Team?.Id ??
                     throw new ArgumentException("Missing Team Id"))
                    {
                        IncludePlayers = true
                    }
                .ExecuteAsync(sports)
                ).Single();

            composer.MapInto(request.Team, entity);

            return new TeamData
            {
                Id = entity.Id
            };
        }
    }
}
