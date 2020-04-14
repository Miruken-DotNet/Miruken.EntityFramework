namespace Miruken.EntityFramework
{
    using System;
    using Register;

    public static class RegistrationExtensions
    {
        public static Registration WithEntityFrameworkCore(
            this Registration registration, Action<EntityFrameworkOptions> configure)
        {
            if (!registration.CanRegister(typeof(RegistrationExtensions)))
                return registration;

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            
            registration.Services(services =>
            {
                var options = new EntityFrameworkOptions(services);
                configure(options);

                if (!options.DefaultOptionsDefined)
                    throw new InvalidOperationException(
                        "A default DbContextOptions type must be specified.  Did you forget to call EntityFrameworkOptions.UseDefaultOptions");
            });

            return registration
                .Sources(sources => sources.FromAssemblyOf<UnitOfWork>())
                .Select((selector, publicOnly) =>
                    selector.AddClasses(x => x.AssignableTo<IDbContext>(), publicOnly)
                        .AsSelf().WithScopedLifetime());
        }
    }
}
