using Analytics.Linq.Core.Test.Models;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceCollection.Extensions.DependencyInjection.Named;

namespace Analytics.Linq.Core.Test
{
    /// <summary>
    /// Reproduces and guards against the "cross-injection" bug: when the SAME concrete
    /// implementation type is registered under two different names, resolving one name
    /// must not silently hand back the instance that belongs to the other name.
    /// </summary>
    public class CrossInjectionTest
    {
        private const string NameA = "uniqueA";
        private const string NameB = "uniqueB";
        private const string NameC = "uniqueC";
        private const string NameD = "uniqueD";

        [Test]
        public void SameImplementation_DifferentNames_SameTService_ResolvesDistinctInstances()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedScoped<IRepository, RepositoryUnique>(NameA);
            services.AddNamedScoped<IRepository, RepositoryUnique>(NameB);

            using ServiceProvider provider = services.BuildServiceProvider();
            using IServiceScope scope = provider.CreateScope();

            var resolvedA = scope.ServiceProvider.GetNamedService<IRepository>(NameA);
            var resolvedB = scope.ServiceProvider.GetNamedService<IRepository>(NameB);

            Assert.That(resolvedA, Is.Not.Null);
            Assert.That(resolvedB, Is.Not.Null);
            Assert.That(ReferenceEquals(resolvedA, resolvedB), Is.False,
                "Resolving by name A returned the same instance as name B - the wrong object was injected.");
            Assert.That(resolvedA.Name, Is.Not.EqualTo(resolvedB.Name));
        }

        [Test]
        public void SameImplementation_DifferentNames_DifferentTService_ResolvesDistinctInstances()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedScoped<IRepository, RepositoryUnique>(NameC);
            services.AddNamedScoped<IRepositoryAlt, RepositoryUnique>(NameD);

            using ServiceProvider provider = services.BuildServiceProvider();
            using IServiceScope scope = provider.CreateScope();

            var resolvedC = scope.ServiceProvider.GetNamedService<IRepository>(NameC);
            var resolvedD = scope.ServiceProvider.GetNamedService<IRepositoryAlt>(NameD);

            Assert.That(resolvedC, Is.Not.Null);
            Assert.That(resolvedD, Is.Not.Null);
            Assert.That(ReferenceEquals(resolvedC, resolvedD), Is.False,
                "Resolving IRepository by name C returned the same instance as IRepositoryAlt by name D.");
            Assert.That(resolvedC.Name, Is.Not.EqualTo(resolvedD.Name));
        }

        [Test]
        public void SameImplementation_SameName_ScopedLifetime_ResolvesSameInstanceWithinScope()
        {
            // Guard against overcorrecting: within the SAME scope, resolving the SAME
            // name twice must still respect the Scoped lifetime (one instance per scope).
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedScoped<IRepository, RepositoryUnique>(NameA);
            services.AddNamedScoped<IRepository, RepositoryUnique>(NameB);

            using ServiceProvider provider = services.BuildServiceProvider();
            using IServiceScope scope = provider.CreateScope();

            var first = scope.ServiceProvider.GetNamedService<IRepository>(NameA);
            var second = scope.ServiceProvider.GetNamedService<IRepository>(NameA);

            Assert.That(ReferenceEquals(first, second), Is.True,
                "Resolving the same name twice within the same scope should return the same scoped instance.");
        }
    }
}
