namespace Miruken.EntityFramework
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Register;

    public static class RegistrationExtensions
    {
        public static Registration WithEntityFrameworkCore(
            this Registration registration, Type defaultDbContextOptions = null)
        {
            if (!registration.CanRegister(typeof(RegistrationExtensions)))
                return registration;

            if (defaultDbContextOptions != null)
            {
                if (!(defaultDbContextOptions.IsGenericTypeDefinition &&
                      typeof(DbContextOptions).IsAssignableFrom(defaultDbContextOptions)))
                {
                    throw new ArgumentException(
                        $"{defaultDbContextOptions.FullName} does represent an open DbContextOptions class");
                }
            }
            else
            {
                defaultDbContextOptions = typeof(SqlServerOptions<>);
            }

            registration.Services(services =>
            {
                services.AddSingleton(typeof(DbContextOptions<>), defaultDbContextOptions);
            });

            return registration
                .Sources(sources => sources.FromAssemblyOf<UnitOfWork>())
                .Select((selector, publicOnly) =>
                    selector.AddClasses(x => x.AssignableTo<IDbContext>(), publicOnly)
                        .AsSelf().WithScopedLifetime());
        }
    }
}
