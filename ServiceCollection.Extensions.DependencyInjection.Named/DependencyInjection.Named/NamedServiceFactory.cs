using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCollection.Extensions.DependencyInjection.Named
{
    /// <summary>
    /// A factory for resolving named services.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    internal class NamedServiceFactory<TService> : INamedServiceFactory
    {
        private readonly ConcurrentDictionary<CompositeStringTypeKey, Func<IServiceProvider, object>> namedServices = new ConcurrentDictionary<CompositeStringTypeKey, Func<IServiceProvider, object>>();

        /// <inheritdoc/>
        public Type ServiceType { get; } = typeof(TService);

        /// <summary>
        /// Registers the given implementation type with the name.
        /// </summary>
        /// <typeparam name="TImplementation">The implemention type.</typeparam>
        /// <param name="name">The name to register the type as.</param>
        /// <param name="lifetime">The lifetime the implementation was registered with.</param>
        /// <param name="constructIndependently">
        /// <c>true</c> when <typeparamref name="TImplementation"/> is already claimed by another
        /// registration (this or a different name/<typeparamref name="TService"/>) - in that case
        /// this named slot must build its OWN instance instead of going through the shared,
        /// type-keyed <see cref="ServiceProviderServiceExtensions.GetService{TImplementation}"/>,
        /// which would otherwise hand back whichever registration the container resolves last.
        /// See docs/architecture/overview.md &lt;caveats&gt;.
        /// </param>
        public void Register<TImplementation>(string name, ServiceLifetime lifetime, bool constructIndependently)
            where TImplementation : class
        {
            Func<IServiceProvider, object> resolver;

            if (!constructIndependently)
            {
                resolver = (sp) => sp.GetService<TImplementation>();
            }
            else if (lifetime == ServiceLifetime.Transient)
            {
                resolver = (sp) => ActivatorUtilities.CreateInstance<TImplementation>(sp);
            }
            else
            {
                // Emulates Scoped/Singleton caching for this one named slot only, keyed by the
                // provider instance (one IServiceProvider per scope, and a single shared instance
                // for the root/singleton provider) - without touching the shared container slot
                // for TImplementation that other named registrations may also be using.
                var instancesByProvider = new ConditionalWeakTable<IServiceProvider, TImplementation>();
                resolver = (sp) => instancesByProvider.GetValue(sp, provider => ActivatorUtilities.CreateInstance<TImplementation>(provider));
            }

            this.namedServices.TryAdd(new CompositeStringTypeKey(name, typeof(TService)), resolver);
        }

        /// <summary>
        /// Registers the given factory delegate with the name.
        /// </summary>
        /// <typeparam name="TImplementation">The implementation type.</typeparam>
        /// <param name="name">The name to register the type as.</param>
        /// <param name="func">The factory delegate that builds the instance.</param>
        /// <param name="lifetime">
        /// The lifetime the delegate was registered with - controls whether/how the resolved
        /// instance is cached, mirroring the caching behavior of
        /// <see cref="Register{TImplementation}(string, ServiceLifetime, bool)"/>. See
        /// docs/architecture/overview.md &lt;caveats&gt;.
        /// </param>
        public void Register<TImplementation>(string name, Func<IServiceProvider, object> func, ServiceLifetime lifetime)
           where TImplementation : class
        {
            Func<IServiceProvider, object> resolver;

            if (lifetime == ServiceLifetime.Transient)
            {
                resolver = func;
            }
            else if (lifetime == ServiceLifetime.Singleton)
            {
                // A single shared instance for the lifetime of the NamedServiceFactory<TService>
                // itself (which is a container singleton), regardless of which IServiceProvider
                // (root or scoped) the resolution came through.
                var singletonLock = new object();
                object singletonInstance = null;
                bool singletonCreated = false;
                resolver = (sp) =>
                {
                    if (!singletonCreated)
                    {
                        lock (singletonLock)
                        {
                            if (!singletonCreated)
                            {
                                singletonInstance = func(sp);
                                singletonCreated = true;
                            }
                        }
                    }

                    return singletonInstance;
                };
            }
            else
            {
                // Emulates Scoped caching for this one named slot only, keyed by the provider
                // instance (one IServiceProvider per scope) - mirrors the ConditionalWeakTable
                // pattern used by the TImplementation overload above.
                var instancesByProvider = new ConditionalWeakTable<IServiceProvider, object>();
                resolver = (sp) => instancesByProvider.GetValue(sp, provider => func(provider));
            }

            this.namedServices.TryAdd(new CompositeStringTypeKey(name, typeof(TService)), resolver);
        }

        /// <summary>
        /// Resolves the service type matching the given name. 
        /// </summary>
        /// <param name="provider">The service provider for retrieving service objects.</param>
        /// <param name="name">The name the service type is registered as.</param>
        /// <returns>The <see cref="TService"/></returns>
        public TService Resolve(IServiceProvider provider, string name) => (TService)this.Resolve(name, provider);

        /// <inheritdoc/>
        public object Resolve(string name, IServiceProvider provider)
        {
            if (!this.namedServices.TryGetValue(new CompositeStringTypeKey(name, typeof(TService)), out Func<IServiceProvider, object> implementation))
            {
                throw new InvalidOperationException($"No service for type {typeof(TService)} named '{name}' has been registered.");
            }

            return implementation(provider);
        }

        /// <summary>
        /// Used for super fast dictionary key lookups.
        /// </summary>
        private readonly struct CompositeStringTypeKey : IEquatable<CompositeStringTypeKey>
        {
            public CompositeStringTypeKey(string name, Type type)
            {
                this.Name = name;
                this.Type = type;
            }

            public string Name { get; }
            public Type Type { get; }

            public override bool Equals(object obj) => obj is CompositeStringTypeKey nameTypeKey && this.Equals(nameTypeKey);

            public bool Equals(CompositeStringTypeKey other) => this.Name == other.Name && EqualityComparer<Type>.Default.Equals(this.Type, other.Type);

            public override int GetHashCode() => HashCode.Combine(this.Name, this.Type);
        }
    }
}
