namespace Miruken.EntityFramework
{
    using Callback;

    public class UnitOfWorkAttribute : FilterAttribute
    {
        public UnitOfWorkAttribute()
            : base(typeof(UnitOfWorkFilter<,>))
        {
        }

        public bool ForceNew         { get; set; }
        public bool BeginTransaction { get; set; }
    }
}
