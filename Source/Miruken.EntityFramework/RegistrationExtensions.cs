namespace Miruken.EntityFramework
{
    using System;
    using Register;

    public static class RegistrationExtensions
    {
        public static Registration WithEntityFrameworkCore(
            this Registration registration, Action<EntityFrameworkSetup> configure)
        {
            if (!registration.CanRegister(typeof(RegistrationExtensions)))
                return registration;

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            
            registration.Services(services =>
            {
                var setup = new EntityFrameworkSetup(services);
                configure(setup);
                setup.Complete();
                registration.Sources(sources => sources.FromAssemblies(setup.Assemblies));
            });

            return registration
                .Sources(sources => sources.FromAssemblyOf<UnitOfWork>())
                .Select((selector, publicOnly) =>
                    selector.AddClasses(x => x.AssignableTo<IDbContext>(), publicOnly)
                        .AsSelf().WithScopedLifetime());
        }
    }
}
