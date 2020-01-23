namespace Miruken.EntityFramework
{
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
            var attribute        = provider as UnitOfWorkAttribute;
            var beginTransaction = attribute?.BeginTransaction == true;

            UnitOfWork unitOfWork = null;

            if (attribute?.ForceNew != true)
            {
                unitOfWork = composer.StashGet<UnitOfWork>();
                if (beginTransaction && unitOfWork?.BeginTransaction == false)
                    unitOfWork = null;
            }

            var owner = unitOfWork == null;
            if (owner)
            {
                unitOfWork = new UnitOfWork(composer, beginTransaction);
                composer.StashPut(unitOfWork);
            }

            try
            {
                var result = await next();

                if (owner)
                    await unitOfWork.CommitAsync();

                return result;
            }
            finally
            {
                if (owner)
                {
                    composer.StashDrop<UnitOfWork>();
                    unitOfWork.Dispose();
                }
            }
        }
    }
}
