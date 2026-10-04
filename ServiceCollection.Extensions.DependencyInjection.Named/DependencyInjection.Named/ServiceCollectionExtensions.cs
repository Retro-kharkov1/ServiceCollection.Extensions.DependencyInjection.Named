using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCollection.Extensions.DependencyInjection.Named
{ 
    public static class ServiceCollectionExtensions
    { 
         
        private static IServiceCollection AddNamed<TService, TImplementation>(this IServiceCollection serviceCollection, 
            string name, 
            ServiceLifetime lifetime)
            where TImplementation : class
        {
            AddNamedServiceImpl<TService, TImplementation>(serviceCollection, name, lifetime);
            return serviceCollection;
        }

        public static IServiceCollection AddNamedScoped<TService, TImplementation>(this IServiceCollection serviceCollection,
           string name)
           where TImplementation : class => serviceCollection.AddNamed<TService, TImplementation>(name, ServiceLifetime.Scoped);

        public static IServiceCollection AddNamedSingleton<TService, TImplementation>(this IServiceCollection serviceCollection,
           string name)
           where TImplementation : class => serviceCollection.AddNamed<TService, TImplementation>(name, ServiceLifetime.Singleton);

        public static IServiceCollection AddNamedTransient<TService, TImplementation>(this IServiceCollection serviceCollection,
           string name)
           where TImplementation : class => serviceCollection.AddNamed<TService, TImplementation>(name, ServiceLifetime.Transient);

        private static IServiceCollection AddNamed<TService>(this IServiceCollection serviceCollection,
            Func<IServiceProvider, TService> func, 
            string name, 
            ServiceLifetime lifetime) 
            where TService: class

        {
            AddNamedServiceImpl<TService>(serviceCollection, func ,name, lifetime);
            return serviceCollection;
        }

        public static IServiceCollection AddNamedTransient<TService>(this IServiceCollection serviceCollection,
            Func<IServiceProvider, TService> func,
            string name)
            where TService : class => serviceCollection.AddNamed<TService>(func, name, ServiceLifetime.Transient);

        public static IServiceCollection AddNamedSingleton<TService>(this IServiceCollection serviceCollection,
          Func<IServiceProvider, TService> func,
          string name)
          where TService : class => serviceCollection.AddNamed<TService>(func, name, ServiceLifetime.Singleton);

        public static IServiceCollection AddNamedScoped<TService>(this IServiceCollection serviceCollection,
          Func<IServiceProvider, TService> func,
          string name)
          where TService : class => serviceCollection.AddNamed<TService>(func, name, ServiceLifetime.Scoped);

        public static object GetNamedService(this IServiceProvider provider, Type type, string name)
        {
            Type genericType = typeof(NamedServiceFactory<>).MakeGenericType(type);
            object factory = provider.GetServices(genericType).LastOrDefault();
            if (factory is null)
                throw new InvalidOperationException($"No service for type {type} named '{name}' has been registered.");

            return (factory as INamedServiceFactory).Resolve(name, provider);
        }

        public static TService GetNamedService<TService>(this IServiceProvider provider, string name) =>
            (TService)provider.GetNamedService(typeof(TService), name);

        private static void AddNamedServiceImpl<TService, TImplementation>(IServiceCollection serviceCollection, string name, ServiceLifetime lifetime)
            where TImplementation : class
        {
            ServiceDescriptor descriptor = serviceCollection.LastOrDefault(x => x.ServiceType == typeof(NamedServiceFactory<TService>));
            if (!(descriptor?.ImplementationInstance is NamedServiceFactory<TService> factory))
            {
                factory = new NamedServiceFactory<TService>();
                serviceCollection.AddSingleton(factory);
            }

            // If TImplementation has already been registered in the container - by an earlier
            // named registration (possibly for a different TService/name), or by the caller
            // directly - resolving through the shared, type-keyed sp.GetService<TImplementation>()
            // would silently hand back THAT OTHER registration's instance instead of this one's
            // (cross-injection). Detect the collision up front and have this named slot build its
            // own instance independently instead of adding another container descriptor for the
            // same concrete type. See docs/architecture/overview.md <caveats>.
            bool implementationAlreadyRegistered = serviceCollection.Any(x => x.ServiceType == typeof(TImplementation));

            factory.Register<TImplementation>(name, lifetime, implementationAlreadyRegistered);

            if (implementationAlreadyRegistered)
                return;

            // We don't want to register using the service descriptor since that would mean multiple TService types
            // would be registered causing resolution problems for non-named registrations.
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    serviceCollection.AddSingleton<TImplementation>();
                    break;
                case ServiceLifetime.Scoped:
                    serviceCollection.AddScoped<TImplementation>();
                    break;
                case ServiceLifetime.Transient:
                    serviceCollection.AddTransient<TImplementation>();
                    break;
            }
        }

        private static void AddNamedServiceImpl<TService>(IServiceCollection serviceCollection,
            Func<IServiceProvider, TService> func,
            string name, 
            ServiceLifetime lifetime) 
            where TService: class
        {
            ServiceDescriptor descriptor = serviceCollection.LastOrDefault(x => x.ServiceType == typeof(NamedServiceFactory<TService>));
            if (!(descriptor?.ImplementationInstance is NamedServiceFactory<TService> factory))
            {
                factory = new NamedServiceFactory<TService>();
                serviceCollection.AddSingleton(factory);
            }

            factory.Register<TService>(name, func, lifetime);

            // We don't want to register using the service descriptor since that would mean multiple TService types
            // would be registered causing resolution problems for non-named registrations.
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    serviceCollection.AddSingleton<TService>(func);
                    break;
                case ServiceLifetime.Scoped:
                    serviceCollection.AddScoped<TService>(func);
                    break;
                case ServiceLifetime.Transient:
                    serviceCollection.AddTransient<TService>(func);
                    break;
            }
        }
    }
}
