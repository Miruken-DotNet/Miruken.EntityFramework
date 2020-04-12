namespace Miruken.EntityFramework.Tests
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Api;
    using Callback;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Miruken.Api;

    public abstract class TransactionTests : DatabaseScenario
    {
        [TestMethod]
        public async Task Should_Require_Existing_Transaction()
        {
            await Context.Send(new Test<RequiredTransactionScenario.Required>());
        }

        [TestMethod]
        public async Task Should_Fail_Required_Mismatched_Isolation_Transaction()
        {
            try
            {
                await Context.Send(new Test<RequiredTransactionScenario.RequiredRepeatableRead>());
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
        public async Task Should_Fail_Requires_New_Inner_Transaction()
        {
            try
            {
                await Context.Send(new Test<RequiredTransactionScenario.RequiresNew>());
                Assert.Fail("Expected to fail");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual(
                    "Inner UnitOfWork required a new transaction.  If this is desired set ForceNew to true.",
                    ex.Message);
            }
        }

        [TestMethod]
        public async Task Should_Require_New_Transaction()
        {
            await Context.Send(new Test<RequiresNewTransactionScenario.Required>());
        }

        [TestMethod]
        public async Task Should_Fail_Requires_New_Transaction()
        {
            try
            {
                await Context.Send(new Test<RequiresNewTransactionScenario.RequiresNew>());
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

    public abstract class TestHandler : Handler
    {
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
    }

    public class Test<TScenario> where TScenario : new()
    {
        public TScenario Scenario { get; } = new TScenario();
    }

    #region Required Scenario

    public class RequiredTransactionScenario
    {
        public class Required    { }
        public class RequiresNew { }

        public class RequiredRepeatableRead { }

        public class Handler : TestHandler
        {
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
    }
    #endregion

    #region Requires New Scenario

    public class RequiresNewTransactionScenario
    {
        public class Required    { }
        public class RequiresNew { }

        public class Handler : TestHandler
        {
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
    }
    #endregion
}
