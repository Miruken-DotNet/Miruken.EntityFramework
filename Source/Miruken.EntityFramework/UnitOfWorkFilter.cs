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
                composer.Resolve<UnitOfWork>(),
                provider as UnitOfWorkAttribute,
                transaction,
                composer);

            try
            {
                var result = await next(unitOfWork + composer);
                await unitOfWork.CommitAsync();
                return result;
            }
            finally
            {
                unitOfWork.Dispose();
            }
        }
    }
}
