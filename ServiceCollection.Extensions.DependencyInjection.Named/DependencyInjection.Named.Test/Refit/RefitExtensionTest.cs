using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using NUnit.Framework;
using Refit;
using ServiceCollection.Extensions.DependencyInjection.Named;
using ServiceCollection.Extensions.DependencyInjection.Named.Refit;
using DependencyInjection.Named.Test.Http;

namespace DependencyInjection.Named.Test.Refit
{
    public class RefitExtensionTest
    {
        private const string NameOne = "refitClientOne";
        private const string NameTwo = "refitClientTwo";

        [Test]
        public void AddNamedRefitClient_ResolvesTypedClientForName()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedRefitClient<IPingApi>(NameOne, new RefitSettings());

            using ServiceProvider provider = services.BuildServiceProvider();

            var client = provider.GetNamedService<IPingApi>(NameOne);

            Assert.That(client, Is.Not.Null);
            Assert.That(client, Is.InstanceOf<IPingApi>());
        }

        [Test]
        public void AddNamedRefitClient_SameInterfaceTwoNames_ResolvesIndependentTypedClients()
        {
            // Regression check for the cross-injection class of bugs: the SAME Refit interface T
            // registered under two different names must not have one name's typed client/settings
            // bleed into the other's.
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedRefitClient<IPingApi>(NameOne, new RefitSettings());
            services.AddNamedRefitClient<IPingApi>(NameTwo, new RefitSettings());

            using ServiceProvider provider = services.BuildServiceProvider();

            var clientOne = provider.GetNamedService<IPingApi>(NameOne);
            var clientTwo = provider.GetNamedService<IPingApi>(NameTwo);

            var settingsOne = provider.GetNamedService<SettingsFor<IPingApi>>(NameOne);
            var settingsTwo = provider.GetNamedService<SettingsFor<IPingApi>>(NameTwo);

            Assert.That(clientOne, Is.Not.Null);
            Assert.That(clientTwo, Is.Not.Null);
            Assert.That(ReferenceEquals(clientOne, clientTwo), Is.False,
                "Resolving by NameOne returned the same typed-client instance as NameTwo.");
            Assert.That(ReferenceEquals(settingsOne, settingsTwo), Is.False,
                "The two names must not share the same SettingsFor<IPingApi> registration.");
        }

        [Test]
        public void AddNamedRefitClient_WithAuthorizationHeaderValueGetter_ConfiguresAuthenticatedHandlerAsPrimary()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            var settings = new RefitSettings
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult("token"),
            };
            services.AddNamedRefitClient<IPingApi>(NameOne, settings);

            using ServiceProvider provider = services.BuildServiceProvider();
            var handlerFactory = provider.GetRequiredService<IHttpMessageHandlerFactory>();

            HttpMessageHandler outerHandler = handlerFactory.CreateHandler(NameOne);
            var chain = HttpClientNamedExtensionsTest.GetHandlerChain(outerHandler).ToList();
            var authHandler = chain.OfType<AuthenticatedHttpClientHandler>().SingleOrDefault();

            Assert.That(authHandler, Is.Not.Null,
                "The pipeline must contain exactly one AuthenticatedHttpClientHandler.");
            Assert.That(authHandler!.InnerHandler, Is.InstanceOf<HttpClientHandler>(),
                "AuthenticatedHttpClientHandler must sit directly on top of the transport handler - " +
                "confirming it was wired as the PRIMARY handler, not an additional one.");
        }

        [Test]
        public void AddNamedRefitClient_WithAuthorizationHeaderValueWithParamGetter_ConfiguresParameterizedAuthenticatedHandlerAsPrimary()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            var settings = new RefitSettings
            {
                AuthorizationHeaderValueWithParamGetter = _ => Task.FromResult("token"),
            };
            services.AddNamedRefitClient<IPingApi>(NameOne, settings);

            using ServiceProvider provider = services.BuildServiceProvider();
            var handlerFactory = provider.GetRequiredService<IHttpMessageHandlerFactory>();

            HttpMessageHandler outerHandler = handlerFactory.CreateHandler(NameOne);
            var chain = HttpClientNamedExtensionsTest.GetHandlerChain(outerHandler).ToList();
            var authHandler = chain.OfType<global::DependencyInjection.Named.Refit.AuthenticatedParameterizedHttpClientHandler>().SingleOrDefault();

            Assert.That(authHandler, Is.Not.Null,
                "The pipeline must contain exactly one AuthenticatedParameterizedHttpClientHandler.");
            Assert.That(authHandler!.InnerHandler, Is.InstanceOf<HttpClientHandler>(),
                "AuthenticatedParameterizedHttpClientHandler must sit directly on top of the transport handler - " +
                "confirming it was wired as the PRIMARY handler, not an additional one.");
        }

        [Test]
        public void AddNamedRefitClient_WithoutAuthorizationGetters_DoesNotConfigureAuthenticatedHandler()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedRefitClient<IPingApi>(NameOne, new RefitSettings());

            using ServiceProvider provider = services.BuildServiceProvider();
            var handlerFactory = provider.GetRequiredService<IHttpMessageHandlerFactory>();

            HttpMessageHandler outerHandler = handlerFactory.CreateHandler(NameOne);
            var chain = HttpClientNamedExtensionsTest.GetHandlerChain(outerHandler).ToList();

            Assert.That(chain.OfType<AuthenticatedHttpClientHandler>(), Is.Empty);
            Assert.That(chain.OfType<global::DependencyInjection.Named.Refit.AuthenticatedParameterizedHttpClientHandler>(), Is.Empty);
        }

        public interface IPingApi
        {
            [Get("/ping")]
            Task<string> Ping();
        }
    }
}
