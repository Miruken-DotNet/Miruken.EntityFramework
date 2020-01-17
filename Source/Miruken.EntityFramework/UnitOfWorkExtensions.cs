namespace Miruken.EntityFramework
{
    using System;
    using System.Threading.Tasks;
    using Api;
    using Callback;

    public static class UnitOfWorkExtensions
    {
        public static UnitOfWork CreateUnitOfWork(this IHandler handler,
            Action<UnitOfWork, IHandler> action)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
         
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var unitOfWork = new UnitOfWork(handler);
            var uowHandler = new Stash() + handler;
            uowHandler.StashPut(unitOfWork);
            action(unitOfWork, uowHandler);
            return unitOfWork;
        }

        public static async Task<UnitOfWork> CreateUnitOfWork(this IHandler handler,
            Func<UnitOfWork, IHandler, Task> action)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var unitOfWork = new UnitOfWork(handler);
            var uowHandler = new Stash() + handler;
            uowHandler.StashPut(unitOfWork);
            await action(unitOfWork, uowHandler);
            return unitOfWork;
        }
    }
}
