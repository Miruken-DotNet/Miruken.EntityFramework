namespace Miruken.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class EntityFrameworkSetup
    {
        private readonly IServiceCollection _services;
        private readonly Dictionary<Type, (int, Type, Type, Type)> _bindings;
        private bool _hasDefaultProviderType;

        public EntityFrameworkSetup(IServiceCollection services)
        {
            _services = services;
            _bindings = new Dictionary<Type, (int, Type, Type, Type)>();
        }

        public EntityFrameworkSetup DbContext<T>()
            where T : DbContextOptions
        {
            return BindDbContext(typeof(T));
        }

        public EntityFrameworkSetup DbContext<T, TC>()
            where T  : DbContextOptions
            where TC : IExtension<T>
        {
           return BindDbContext(typeof(T), typeof(TC));
        }

        public EntityFrameworkSetup DbContext<T, TC>(TC configuration)
            where T : DbContextOptions
            where TC : class, IExtension<T>
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            BindDbContext(typeof(T));
            _services.AddSingleton(configuration);
            return this;
        }

        public EntityFrameworkSetup DbContext(
            Type dbContextProvider,
            Type dbContextConfiguration = null)
        {
            return BindDbContext(dbContextProvider, dbContextConfiguration);
        }

        internal void Complete()
        {
            foreach (var (_, provider, configuration, openConfiguration)
                in _bindings.Values.OrderBy(x => x.Item1))
            {
                _services.AddSingleton(provider);
                if (configuration != null)
                {
                    if (configuration.IsGenericTypeDefinition)
                        _services.AddSingleton(openConfiguration, configuration);
                    else
                        _services.AddSingleton(configuration);
                }
            }
        }

        private EntityFrameworkSetup BindDbContext(
            Type dbContextProviderType,
            Type dbContextConfigurationType = null)
        {
            if (dbContextProviderType == null)
                throw new ArgumentNullException(nameof(dbContextProviderType));

            if (dbContextProviderType.IsGenericTypeDefinition)
            {
                if (_hasDefaultProviderType)
                    throw new InvalidOperationException("Only one default DbContextOptions<> is allowed");
                _hasDefaultProviderType = true;
            }

            var optionsType = dbContextProviderType.GetOpenTypeConformance(typeof(DbContextOptions<>));
            if (optionsType == null)
            {
                throw new ArgumentException(
                    $"'{dbContextProviderType}' must be a generic DbContextOptions<> class");
            }
            var dbContextType = optionsType.GetGenericArguments()[0];
            if (_bindings.TryGetValue(dbContextType, out var binding))
            {
                throw new InvalidOperationException(
                    $"DbContext '{dbContextType}' is already bound to '{binding.Item2.Name}'");
            }

            Type configurationType = null;
            if (dbContextConfigurationType != null)
            {
                configurationType = FindConfigurationType(dbContextProviderType);
                if (configurationType == null)
                {
                    throw new ArgumentException(
                        $"'{dbContextProviderType.FullName}' does not define a nested Configuration class");
                }

                if (configurationType.IsGenericTypeDefinition)
                {
                    var conformance = dbContextConfigurationType.GetOpenTypeConformance(configurationType);
                    if (conformance == null)
                    {
                        throw new ArgumentException(
                            $"'{dbContextConfigurationType.FullName}' does not conform to Configuration type '{configurationType.FullName}'");
                    }
                }
                else if (!configurationType.IsAssignableFrom(dbContextConfigurationType))
                {
                    throw new ArgumentException(
                        $"'{dbContextConfigurationType.FullName}' is not an instance of Configuration type '{configurationType.FullName}'");
                }
            }

            _bindings.Add(dbContextType,
                (
                    dbContextProviderType.IsGenericTypeDefinition ? 0 : _bindings.Count + 1,
                    dbContextProviderType,
                    dbContextConfigurationType,
                    configurationType)
                );

            return this;
        }

        private static Type FindConfigurationType(Type dbContextProviderType) =>
            dbContextProviderType.GetNestedTypes().First(t => t.Name == "Configuration");
    }
}
