namespace Miruken.EntityFramework.Tests.Api
{
    using System.Linq;
    using Callback;
    using Domain;
    using Map;

    public class TeamMapping : Handler
    {
        [Maps]
        public static TeamData Map(
            Team     entity,
            TeamData data,
            IHandler composer)
        {
            data ??= new TeamData();
            data.Id      = entity.Id;
            data.Name    = entity.Name;
            data.Coach   = entity.Coach != null
                         ? composer.Map<PersonData>(entity.Coach) 
                         : null;
            data.Players = composer.MapAll<PersonData>(entity.Players);
            return data;
        }

        [Maps]
        public static Team Map(
            TeamData       data,
            Team           entity,
            ISportsContext sports,
            IHandler       composer)
        {
            entity ??= new Team();

            if (data.Name != null)
                entity.Name = data.Name;

            if (data.Coach != null)
            {
                entity.Coach = entity.Coach != null
                             ? composer.MapInto(data.Coach, entity.Coach)
                             : composer.Map<Person>(data.Coach);
            }

            MapPlayers(entity, data.Players, sports, composer);

            return entity;
        }

        private static void MapPlayers(
            Team           team,
            PersonData[]   players,
            ISportsContext sports,
            IHandler       composer)
        {
            if (players == null) return;

            if (team.Id > 0)
            {
                sports.Entry(team)
                    ?.Collection(b => b.Players)
                    ?.Load();
            }

            var playerActions = players.ToLookup(d =>
            {
                if (d.Id == null) return 1;
                return d.Id < 0 ? -1 : 0;
            });

            if (playerActions.Contains(-1))
            {
                var removePlayers = playerActions[-1].SelectMany(tt =>
                        team.Players.Where(t => t.Id == -tt.Id))
                    .ToArray();

                foreach (var removePlayer in removePlayers)
                {
                    team.Players.Remove(removePlayer);
                }
            }

            if (playerActions.Contains(1))
            {
                foreach (var addPlayer in playerActions[1])
                    team.Players.Add(composer.Map<Person>(addPlayer));
            }

            if (playerActions.Contains(0))
            {
                foreach (var player in team.Players)
                {
                    var data = players.FirstOrDefault(x => x.Id == player.Id);
                    if (data != null)
                        composer.Map(data, player);
                }
            }
        }
    }
}
