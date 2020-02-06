namespace Miruken.EntityFramework.Tests
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Api;
    using Callback;
    using Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Miruken.Api;
    using Register;
    using ServiceCollection = Register.ServiceCollection;

    public abstract class TransactionScenario
    {
        protected Context Context;
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

        protected static Task<LeagueResult> CreateLeague(IHandler composer)
        {
            return composer.Send(new CreateLeague
            {
                Teams = new[]
                {
                    new TeamData { Name = "Breakaway" }
                }
            });
        }

        public class Test<TScenario> where TScenario : new()
        {
            public TScenario Scenario { get; } = new TScenario();
        }
    }

    #region Required Scenario

    [TestClass]
    public class RequiredTransactionScenario : TransactionScenario
    {
        public class Suppress    { }
        public class Required    { }
        public class RequiresNew { }

        public class RequiredRepeatableRead { }

        public class Handler
        {
// Suppress
            [Handles, UnitOfWork, Transaction]
            public Task InnerSuppress(Test<Suppress> test, IHandler composer)
            {
                return composer.Send(test.Scenario);
            }

            [Handles, UnitOfWork, Transaction(TransactionOption.Suppress)]
            public Task TestSuppress(Suppress _, IHandler composer)
            {
                return CreateLeague(composer);
            }

// Required
            [Handles, UnitOfWork, Transaction]
            public Task InnerRequired(Test<Required> test, IHandler composer)
            {
                return composer.Send(test.Scenario);
            }

            [Handles, UnitOfWork, Transaction]
            public Task TestRequired(Required _, IHandler composer)
            {
                return CreateLeague(composer);
            }


// Required Repeatable Read
            [Handles, UnitOfWork, Transaction]
            public Task InnerRequiredRepeatableReader(
                Test<RequiredRepeatableRead> test, IHandler composer)
            {
                return composer.Send(test.Scenario);
            }

            [Handles, UnitOfWork, Transaction(IsolationLevel.RepeatableRead)]
            public Task TestRequiredRepeatableRead(
                RequiredRepeatableRead _, IHandler composer)
            {
                return CreateLeague(composer);
            }

            // Requires New
            [Handles, UnitOfWork, Transaction]
            public Task InnerRequiresNew(Test<RequiresNew> test, IHandler composer)
            {
                return composer.Send(test.Scenario);
            }

            [Handles, UnitOfWork, Transaction(TransactionOption.RequiresNew)]
            public Task TestRequiresNew(RequiresNew _, IHandler composer)
            {
                return CreateLeague(composer);
            }
        }

        [TestMethod]
        public async Task Should_Require_Transaction()
        {
            await Context.Send(new Test<Required>());
        }

        [TestMethod]
        public async Task Should_Fail_Required_Mismatched_Isolation_Transaction()
        {
            try
            {
                await Context.Send(new Test<RequiredRepeatableRead>());
                Assert.Fail("Expected to fail");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(
                    "Inner UnitOfWork required Isolation 'RepeatableRead', but the outer transaction has Isolation 'Default'.  If this is desired set ForceNew to true.",
                    ex.Message);
            }
        }

        [TestMethod]
        public async Task Should_Fail_Suppress_Transaction()
        {
            try
            {
                await Context.Send(new Test<Suppress>());
                Assert.Fail("Expected to fail");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(
                    "Inner UnitOfWork requested to suppress transactions, but an outer transaction has already started.  If this is desired set ForceNew to true.",
                    ex.Message);
            }
        }

        [TestMethod]
        public async Task Should_Fail_Requires_New_Transaction()
        {
            try
            {
                await Context.Send(new Test<RequiresNew>());
                Assert.Fail("Expected to fail");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(
                    "Inner UnitOfWork required a new transaction.  If this is desired set ForceNew to true.",
                    ex.Message);
            }
        }
    }
    #endregion

    #region Requires New Scenario

    [TestClass]
    public class RequiresNewTransactionScenario : TransactionScenario
    {
        public class Suppress    { }
        public class Required    { }
        public class RequiresNew { }

        public class Handler
        {
// Suppress
            [Handles, UnitOfWork, Transaction(TransactionOption.RequiresNew)]
            public Task InnerSuppress(Test<Suppress> test, IHandler composer)
            {
                return composer.Send(test.Scenario);
            }

            [Handles, UnitOfWork, Transaction(TransactionOption.Suppress)]
            public Task TestSuppress(Suppress _, IHandler composer)
            {
                return CreateLeague(composer);
            }

// Required
            [Handles, UnitOfWork, Transaction(TransactionOption.RequiresNew)]
            public Task InnerRequired(Test<Required> test, IHandler composer)
            {
                return composer.Send(test.Scenario);
            }

            [Handles, UnitOfWork, Transaction]
            public Task TestRequired(Required _, IHandler composer)
            {
                return CreateLeague(composer);
            }

// Requires New
            [Handles, UnitOfWork, Transaction(TransactionOption.RequiresNew)]
            public Task InnerRequiresNew(Test<RequiresNew> test, IHandler composer)
            {
                return composer.Send(test.Scenario);
            }

            [Handles, UnitOfWork, Transaction(TransactionOption.RequiresNew)]
            public Task TestRequiresNew(RequiresNew _, IHandler composer)
            {
                return CreateLeague(composer);
            }
        }

        [TestMethod]
        public async Task Should_Require_Transaction()
        {
            await Context.Send(new Test<Required>());
        }

        [TestMethod]
        public async Task Should_Fail_Suppress_Transaction()
        {
            try
            {
                await Context.Send(new Test<Suppress>());
                Assert.Fail("Expected to fail");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(
                    "Inner UnitOfWork requested to suppress transactions, but an outer transaction has already started.  If this is desired set ForceNew to true.",
                    ex.Message);
            }
        }

        [TestMethod]
        public async Task Should_Fail_Requires_New_Transaction()
        {
            try
            {
                await Context.Send(new Test<RequiresNew>());
                Assert.Fail("Expected to fail");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(
                    "Inner UnitOfWork required a new transaction.  If this is desired set ForceNew to true.",
                    ex.Message);
            }
        }
    }
    #endregion

    #region Suppress Scenario

    [TestClass]
    public class SuppressTransactionScenario : TransactionScenario
    {
        public class Suppress    { }
        public class Required    { }
        public class RequiresNew { }

        public class Handler
        {
            // Suppress
            [Handles, UnitOfWork, Transaction(TransactionOption.Suppress)]
            public Task InnerSuppress(Test<Suppress> test, IHandler composer)
            {
                return composer.Send(test.Scenario);
            }

            [Handles, UnitOfWork, Transaction(TransactionOption.Suppress)]
            public Task TestSuppress(Suppress _, IHandler composer)
            {
                return CreateLeague(composer);
            }

            // Required
            [Handles, UnitOfWork, Transaction(TransactionOption.Suppress)]
            public Task InnerRequired(Test<Required> test, IHandler composer)
            {
                return composer.Send(test.Scenario);
            }

            [Handles, UnitOfWork, Transaction]
            public Task TestRequired(Required _, IHandler composer)
            {
                return CreateLeague(composer);
            }

            // Requires New
            [Handles, UnitOfWork, Transaction(TransactionOption.Suppress)]
            public Task InnerRequiresNew(Test<RequiresNew> test, IHandler composer)
            {
                return composer.Send(test.Scenario);
            }

            [Handles, UnitOfWork, Transaction(TransactionOption.RequiresNew)]
            public Task TestRequiresNew(RequiresNew _, IHandler composer)
            {
                return CreateLeague(composer);
            }
        }

        [TestMethod]
        public async Task Should_Suppress_Transaction()
        {
            await Context.Send(new Test<Suppress>());
        }

        [TestMethod]
        public async Task Should_Fail_Required_Transaction()
        {
            try
            {
                await Context.Send(new Test<Required>());
                Assert.Fail("Expected to fail");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(
                    "Inner UnitOfWork requested a Transaction, but the outer did not.  If this is desired set ForceNew to true.",
                    ex.Message);
            }
        }

        [TestMethod]
        public async Task Should_Fail_Requires_New_Transaction()
        {
            try
            {
                await Context.Send(new Test<RequiresNew>());
                Assert.Fail("Expected to fail");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(
                    "Inner UnitOfWork requested a Transaction, but the outer did not.  If this is desired set ForceNew to true.",
                    ex.Message);
            }
        }
    }
    #endregion
}
