using Analytics.Linq.Core.Test.Models;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ServiceCollection.Extensions.DependencyInjection.Named;
using System;

namespace Analytics.Linq.Core.Test
{
    /// <summary>
    /// Covers lifetime correctness (singleton vs transient) for the <c>AddNamed*</c> overloads,
    /// and the observable behavior of <see cref="ServiceCollectionExtensions.GetNamedService{TService}"/>
    /// when the requested name/type was never registered, or was registered twice.
    /// </summary>
    public class NamedServiceLifetimeTest
    {
        private const string NameA = "lifetimeA";
        private const string NameB = "lifetimeB";
        private const string Unregistered = "never-registered";

        [Test]
        public void AddNamedSingleton_TServiceTImplementation_ReturnsSameInstanceAcrossResolutionsAndScopes()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedSingleton<IRepository, RepositoryUnique>(NameA);

            using ServiceProvider provider = services.BuildServiceProvider();

            var rootResolution1 = provider.GetNamedService<IRepository>(NameA);
            var rootResolution2 = provider.GetNamedService<IRepository>(NameA);

            using IServiceScope scope = provider.CreateScope();
            var scopedResolution = scope.ServiceProvider.GetNamedService<IRepository>(NameA);

            Assert.That(ReferenceEquals(rootResolution1, rootResolution2), Is.True,
                "A singleton registration must return the same instance on repeated resolution.");
            Assert.That(ReferenceEquals(rootResolution1, scopedResolution), Is.True,
                "A singleton registration must return the same instance across scopes.");
        }

        [Test]
        public void AddNamedTransient_TServiceTImplementation_ReturnsNewInstanceEveryResolution()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedTransient<IRepository, RepositoryUnique>(NameA);

            using ServiceProvider provider = services.BuildServiceProvider();

            var first = provider.GetNamedService<IRepository>(NameA);
            var second = provider.GetNamedService<IRepository>(NameA);

            Assert.That(ReferenceEquals(first, second), Is.False,
                "A transient registration must return a new instance on every resolution.");
        }

        [Test]
        public void AddNamedSingleton_FuncOverload_ReturnsSameInstanceAcrossResolutions()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedSingleton<IRepository>(sp => new Repository(Guid.NewGuid().ToString()), NameB);

            using ServiceProvider provider = services.BuildServiceProvider();

            var first = provider.GetNamedService<IRepository>(NameB);
            var second = provider.GetNamedService<IRepository>(NameB);

            Assert.That(ReferenceEquals(first, second), Is.True,
                "A singleton Func-based registration must return the same instance on repeated resolution.");
        }

        [Test]
        public void AddNamedTransient_FuncOverload_ReturnsNewInstanceEveryResolution()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedTransient<IRepository>(sp => new Repository(Guid.NewGuid().ToString()), NameB);

            using ServiceProvider provider = services.BuildServiceProvider();

            var first = provider.GetNamedService<IRepository>(NameB);
            var second = provider.GetNamedService<IRepository>(NameB);

            Assert.That(ReferenceEquals(first, second), Is.False,
                "A transient Func-based registration must return a new instance on every resolution.");
            Assert.That(first.Name, Is.Not.EqualTo(second.Name));
        }

        [Test]
        public void GetNamedService_NoFactoryRegisteredForType_ThrowsInvalidOperationException()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            using ServiceProvider provider = services.BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                provider.GetNamedService<IRepository>(Unregistered));

            Assert.That(exception!.Message, Does.Contain(Unregistered));
        }

        [Test]
        public void GetNamedService_FactoryExistsButNameNotRegistered_ThrowsInvalidOperationException()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedSingleton<IRepository, RepositoryUnique>(NameA);

            using ServiceProvider provider = services.BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                provider.GetNamedService<IRepository>(Unregistered));

            Assert.That(exception!.Message, Does.Contain(Unregistered));
        }

        [Test]
        public void AddNamedSingleton_SameServiceAndNameRegisteredTwice_FirstRegistrationWins()
        {
            // Characterizes the current, intentional behavior: the internal named-service
            // registry uses TryAdd keyed by (name, TService), so a second registration for the
            // SAME (TService, name) pair is silently ignored. If this ever needs to change to
            // "last one wins" or "throw on duplicate", this test must be updated deliberately.
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedSingleton<IRepository, RepositoryOne>(NameA);
            services.AddNamedSingleton<IRepository, RepositoryTwo>(NameA);

            using ServiceProvider provider = services.BuildServiceProvider();

            var resolved = provider.GetNamedService<IRepository>(NameA);

            Assert.That(resolved, Is.InstanceOf<RepositoryOne>(),
                "The first registration for a given (TService, name) pair must win over later ones.");
        }
    }
}
