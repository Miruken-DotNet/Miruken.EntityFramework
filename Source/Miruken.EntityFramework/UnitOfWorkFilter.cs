namespace Miruken.EntityFramework
{
    using System.Linq;
    using System.Threading.Tasks;
    using Api;
    using Callback;
    using Callback.Policy.Bindings;

    public class UnitOfWorkFilter<Cb, Res> : IFilter<Cb, Res>
    {
        public int? Order { get; set; } = Stage.Authorization - 1;

        public async Task<Res> Next(Cb callback,
            object rawCallback, MemberBinding member,
            IHandler composer, Next<Res> next,
            IFilterProvider provider)
        {
            var transaction = member.Dispatcher.Attributes
                .OfType<TransactionAttribute>().SingleOrDefault();

            var unitOfWork = new UnitOfWork(
                composer.StashGet<UnitOfWork>(),
                provider as UnitOfWorkAttribute,
                transaction,
                composer);

            var stash = new Stash();
            stash.StashPut(unitOfWork);

            try
            {
                var result = await next(stash + composer);
                await unitOfWork.CommitAsync();
                return result;
            }
            finally
            {
                stash.StashDrop<UnitOfWork>();
                unitOfWork.Dispose();
            }
        }
    }
}
