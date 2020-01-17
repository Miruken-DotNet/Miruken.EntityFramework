namespace Miruken.EntityFramework
{
    using Callback;

    public class UnitOfWorkAttribute : FilterAttribute
    {
        public UnitOfWorkAttribute()
            : base(typeof(UnitOfWorkFilter<,>))
        {
        }
    }
}
