namespace Miruken.EntityFramework.Tests
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Api;
    using Context;
    using Domain;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Miruken.Api;
    using Register;
    using ServiceCollection = Register.ServiceCollection;

    [TestClass]
    public class UnitOfWorkTests
    {
        private Context Context;
        private DbContextOptions<SportsContext> _options;
        private SportsContext _context;

        [TestInitialize]
        public void TestInitialize()
        {
            var builder = new DbContextOptionsBuilder<SportsContext>()
                .UseSqlServer(
                    $"Server=(LocalDB)\\MSSQLLocalDB;Database=sports_db_{Guid.NewGuid()};Trusted_Connection=True;MultipleActiveResultSets=true");
            _options = builder.Options;

            _context = new SportsContext(_options);
            _context.Database.EnsureCreated();

            Context = new ServiceCollection()
                .AddSingleton(_options)
                .AddMiruken(configure =>
                {
                    configure
                        .PublicSources(sources => sources.FromAssemblyOf<UnitOfWorkTests>())
                        .WithEntityFrameworkCore();
                }).Build();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            using (_context)
            {
                _context.Database.EnsureDeleted();
            }

            Context.End();
        }

        [TestMethod]
        public async Task Should_Create_Read_Update()
        {
            var team = new Team
            {
                Name = "Craig"
            };

            await using (var context = new SportsContext(_options))
            {
                await using var transaction = context.Database.BeginTransaction();
                context.Add(team);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            await using (var context = new SportsContext(_options))
            {
                var fetchTeam = (await new QueryTeam.ById(team.Id)
                    .ExecuteAsync(context)).Single();
                fetchTeam.Name = "Matthew";
                await context.SaveChangesAsync();
                await using var transaction = context.Database.BeginTransaction();
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            await using (var context = new SportsContext(_options))
            {
                var fetchTeam = (await new QueryTeam.ById(team.Id)
                    .ExecuteAsync(context)).Single();
                Assert.AreEqual("Matthew", fetchTeam.Name);
            }

            await using (var context = new SportsContext(_options))
            {
                await using var transaction = context.Database.BeginTransaction();
                var fetchTeam = (await new QueryTeam.ById(team.Id)
                    .ExecuteAsync(context)).Single();
                fetchTeam.Name = "John";
                await context.SaveChangesAsync();
                await using (var context2 = new SportsContext(_options))
                {
                    await using var transaction2 = context2.Database.BeginTransaction(
                        IsolationLevel.ReadCommitted);
                    var fetchTeam2 = (await new QueryTeam.ById(team.Id)
                        .ExecuteAsync(context2)).Single();
                    Assert.AreEqual("Matthew", fetchTeam2.Name);
                    await transaction2.CommitAsync();
                }

                await transaction.CommitAsync();
            }
        }

        [TestMethod]
        public async Task Should_Create_In_UnitOfWork()
        {
            var team = await Context.Send(new CreateTeam
            {
                Team = new TeamData
                {
                    Name = "Breakaway",
                    Coach = new PersonData
                    {
                        FirstName = "Doug",
                        LastName = "Collins",
                        DateOfBirth = new DateTime(1978, 5, 20)
                    },
                    Players = new[]
                    {
                        new PersonData
                        {
                            FirstName = "Austin",
                            LastName = "Branch",
                            DateOfBirth = new DateTime(2007, 3, 8)
                        },
                        new PersonData
                        {
                            FirstName = "Thomas",
                            LastName = "Smith",
                            DateOfBirth = new DateTime(2008, 4, 15)
                        }
                    }
                }
            });

            Assert.IsNotNull(team);
            Assert.IsNotNull(team.Id);

            var fetchTeam = (await Context.Send(new GetTeams(team.Id.Value)
                {
                    IncludePlayers = true
                }))
                .Teams.FirstOrDefault();

            Assert.IsNotNull(fetchTeam);
            Assert.AreEqual("Breakaway", fetchTeam.Name);
            Assert.AreEqual("Doug", fetchTeam.Coach.FirstName);
            Assert.AreEqual("Collins", fetchTeam.Coach.LastName);
            Assert.AreEqual(new DateTime(1978, 5, 20), fetchTeam.Coach.DateOfBirth);

            var players = fetchTeam.Players;
            Assert.AreEqual(2, players.Length);
            Assert.IsTrue(players.Any(p => p.FirstName == "Austin" && p.LastName == "Branch"));
            Assert.IsTrue(players.Any(p => p.FirstName == "Thomas" && p.LastName == "Smith"));
        }

        [TestMethod]
        public async Task Should_Update_In_UnitOfWork()
        {
            var team = await Context.Send(new CreateTeam
            {
                Team = new TeamData
                {
                    Name = "JuventusXX",
                    Coach = new PersonData
                    {
                        FirstName = "Maurizio",
                        LastName = "Sarri",
                        DateOfBirth = new DateTime(1960, 1, 10)
                    },
                    Players = new[]
                    {
                        new PersonData
                        {
                            FirstName = "Cristiano",
                            LastName = "Ronaldo",
                            DateOfBirth = new DateTime(1985, 2, 5)
                        },
                        new PersonData
                        {
                            FirstName = "Paulo",
                            LastName = "Dybala",
                            DateOfBirth = new DateTime(1993, 11, 15)
                        }
                    }
                }
            });

            Assert.IsNotNull(team.Id);

            var fetchTeam = (await Context.Send(new GetTeams(team.Id.Value)
                {
                    IncludePlayers = true
                }))
                .Teams.FirstOrDefault();

            Assert.IsNotNull(fetchTeam);
            Assert.AreEqual("JuventusXX", fetchTeam.Name);

            var ronaldo = fetchTeam.Players.Single(p => p.LastName == "Ronaldo");

            var updateTeam = new TeamData
            {
                Id = team.Id,
                Name = "Juventus",
                Coach = new PersonData
                {
                    Id = fetchTeam.Coach.Id,
                    DateOfBirth = new DateTime(1959, 1, 10)
                },
                Players = new[] {new PersonData {Id = -ronaldo.Id}}
            };

            await Context.Send(new UpdateTeam {Team = updateTeam});

            var updatedTeam = (await Context.Send(new GetTeams(team.Id.Value)
                {
                    IncludePlayers = true
                }))
                .Teams.FirstOrDefault();

            Assert.IsNotNull(updatedTeam);
            Assert.AreEqual(fetchTeam.Id, updatedTeam.Id);
            Assert.AreEqual("Juventus", updatedTeam.Name);
            Assert.AreEqual("Maurizio", updatedTeam.Coach.FirstName);
            Assert.AreEqual("Sarri", updatedTeam.Coach.LastName);
            Assert.AreEqual(new DateTime(1959, 1, 10), updatedTeam.Coach.DateOfBirth);

            var players = updatedTeam.Players;
            Assert.AreEqual(1, players.Length);
            Assert.IsTrue(players.Any(p => p.FirstName == "Paulo" && p.LastName == "Dybala"));
        }

        [TestMethod]
        public async Task Should_Create_Nested_UnitOfWork()
        {
            var teams = (await Context.Send(new CreateLeague
            {
                Teams = new []
                {
                    new TeamData
                    {
                        Name = "Falcao Juniors"
                    },
                    new TeamData
                    {
                        Name = "Futballers"
                    },
                    new TeamData
                    {
                        Name = "Joga Bonito"
                    }
                }
            })).Teams;

            Assert.IsTrue(teams.All(team => team.Id > 0));
        }
    }
}
