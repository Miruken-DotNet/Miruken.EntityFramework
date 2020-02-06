namespace Miruken.EntityFramework
{
    using System;
    using System.Threading.Tasks;
    using Api;
    using Callback;

    public static class UnitOfWorkExtensions
    {
        private static readonly UnitOfWorkAttribute ForceNew
            = new UnitOfWorkAttribute { ForceNew = true };

        public static UnitOfWork CreateUnitOfWork(
            this IHandler                handler,
            Action<UnitOfWork, IHandler> action, 
            TransactionAttribute         transaction = null)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
         
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var uowHandler = new Stash() + handler;
            var parent     = handler.StashGet<UnitOfWork>();
            var unitOfWork = new UnitOfWork(parent, ForceNew, transaction, uowHandler);
            uowHandler.StashPut(unitOfWork);
            action(unitOfWork, uowHandler);
            return unitOfWork;
        }

        public static async Task<UnitOfWork> CreateUnitOfWork(
            this IHandler                    handler,
            Func<UnitOfWork, IHandler, Task> action,
            TransactionAttribute             transaction = null)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var uowHandler = new Stash() + handler;
            var parent     = handler.StashGet<UnitOfWork>();
            var unitOfWork = new UnitOfWork(parent, ForceNew, transaction, uowHandler);
            uowHandler.StashPut(unitOfWork);
            await action(unitOfWork, uowHandler);
            return unitOfWork;
        }
    }
}
