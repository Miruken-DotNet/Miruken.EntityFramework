namespace Miruken.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class EntityFrameworkSetup
    {
        private readonly IServiceCollection _services;
        private readonly Dictionary<Type, (int, Type, Type, Type)> _bindings;
        private readonly HashSet<Assembly> _assemblies;
        private bool _hasDefaultProviderType;

        public EntityFrameworkSetup(IServiceCollection services)
        {
            _services   = services;
            _bindings   = new Dictionary<Type, (int, Type, Type, Type)>();
            _assemblies = new HashSet<Assembly>();
        }

        public IEnumerable<Assembly> Assemblies => _assemblies;
        
        public EntityFrameworkSetup DbContext<T>(
            Action<IServiceCollection> action = null)
            where T : DbContextOptions
        {
            return BindDbContext(typeof(T), null, action);
        }

        public EntityFrameworkSetup DbContext<T, TC>(
            Action<IServiceCollection> action = null)
            where T  : DbContextOptions
            where TC : IExtension<T>
        {
           return BindDbContext(typeof(T), typeof(TC), action);
        }
        
        public EntityFrameworkSetup DbContext(
            Type dbContextProvider,
            Type dbContextConfiguration = null,
            Action<IServiceCollection> action = null)
        {
            return BindDbContext(dbContextProvider, dbContextConfiguration, action);
        }

        internal void Complete()
        {
            foreach (var (_, provider, configuration, openConfiguration)
                in _bindings.Values.OrderBy(x => x.Item1))
            {
                _services.AddSingleton(provider);
                if (configuration == null) continue;
                if (configuration.IsGenericTypeDefinition)
                    _services.AddSingleton(openConfiguration, configuration);
                else
                    _services.AddSingleton(configuration);
            }
        }

        private EntityFrameworkSetup BindDbContext(
            Type dbContextProviderType,
            Type dbContextConfigurationType,
            Action<IServiceCollection> action)
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
                    // Ensure default (open) provider comes first
                    dbContextProviderType.IsGenericTypeDefinition ? 0 : _bindings.Count + 1,
                    dbContextProviderType,
                    dbContextConfigurationType,
                    configurationType)
                );

            _assemblies.Add(dbContextProviderType.Assembly);
            
            action?.Invoke(_services);
            
            return this;
        }

        private static Type FindConfigurationType(Type dbContextProviderType) =>
            dbContextProviderType.GetNestedTypes().First(t => t.Name == "Configuration");
    }
}
