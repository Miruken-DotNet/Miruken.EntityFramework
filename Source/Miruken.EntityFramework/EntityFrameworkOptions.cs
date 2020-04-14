namespace Miruken.EntityFramework
{
    using System;
    using System.Linq;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class EntityFrameworkOptions
    {
        private readonly IServiceCollection _services;

        public EntityFrameworkOptions(IServiceCollection services)
        {
            _services = services;
        }

        internal bool DefaultOptionsDefined { get; private set; }

        public EntityFrameworkOptions UseDefaultOptions(
            Type defaultDbContextOptions,
            Type defaultDbContextOptionsConfiguration = null)
        {
            if (defaultDbContextOptions == null)
                throw new ArgumentNullException(nameof(defaultDbContextOptions));

            if (!(defaultDbContextOptions.IsGenericTypeDefinition &&
                  typeof(DbContextOptions).IsAssignableFrom(defaultDbContextOptions)))
            {
                throw new ArgumentException(
                    $"'{defaultDbContextOptions.FullName}' does represent an open DbContextOptions class");
            }

            _services.AddSingleton(typeof(DbContextOptions<>), defaultDbContextOptions);

            if (defaultDbContextOptionsConfiguration != null)
                RegisterDbContextOptionsConfiguration(defaultDbContextOptions, defaultDbContextOptionsConfiguration);

            DefaultOptionsDefined = true;

            return this;
        }

        public EntityFrameworkOptions UseDbContextOptions<T, TO>()
            where T  : DbContext
            where TO : DbContextOptions<T>
        {
            _services.AddSingleton<TO>();
            return this;
        }

        public EntityFrameworkOptions UseDbContextOptions<T, TO, TOC>()
            where T   : DbContext
            where TO  : DbContextOptions<T>
        {
            _services.AddSingleton<TO>();
            RegisterDbContextOptionsConfiguration(typeof(TO), typeof(TOC));
            return this;
        }

        private void RegisterDbContextOptionsConfiguration(
            Type dbContextOptions,
            Type dbContextOptionsConfiguration)
        {
            var optionsType = FindOptionsType(dbContextOptions);
            if (optionsType == null)
            {
                throw new ArgumentException(
                    $"'{dbContextOptions.FullName}' does not define a nested Options class");
            }

            if (optionsType.IsGenericTypeDefinition)
            {
                var conformance = dbContextOptionsConfiguration.GetOpenTypeConformance(optionsType);
                if (conformance == null)
                {
                    throw new ArgumentException(
                        $"'{dbContextOptionsConfiguration.FullName}' does not conform to open Options type '{optionsType.FullName}'");
                }
            }
            else if (!optionsType.IsAssignableFrom(dbContextOptionsConfiguration))
            {
                throw new ArgumentException(
                    $"'{dbContextOptionsConfiguration.FullName}' is not an instance of the Options type '{optionsType.FullName}'");
            }

            _services.AddSingleton(optionsType, dbContextOptionsConfiguration);
        }

        private static Type FindOptionsType(Type defaultDbContextOptions) =>
            defaultDbContextOptions.GetNestedTypes()
                .First(t => t.Name == "Options");
    }
}
