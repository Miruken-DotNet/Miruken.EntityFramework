namespace Miruken.EntityFramework.Tests.Api
{
    using System.Linq;
    using Callback;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Miruken.Api;

    public class LeagueHandler
    {
        [Handles, UnitOfWork]
        public LeagueResult Create(
            CreateLeague   request,
            ISportsContext sports,
            IHandler       composer)
        {
            var result = new LeagueResult
            {
                Teams = request.Teams.Select(team => 
                        composer.Send(new CreateTeam { Team = team }).Wait())
                    .ToArray()
            };

            var entries = sports.ChangeTracker?.Entries().ToArray();
            var teams   = entries?.Select(entry => entry.Entity).OfType<Team>().ToArray();
            Assert.IsNotNull(teams);
            Assert.AreEqual(request.Teams.Length, teams.Length);
            Assert.IsTrue(entries.All(entry => entry.State == EntityState.Added));

            return result;
        }
    }
}
