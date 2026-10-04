using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ServiceCollection.Extensions.DependencyInjection.Named;
using Analytics.Linq.Core.Test;

namespace DependencyInjection.Named.Test.Http
{
    public class HttpClientNamedExtensionsTest
    {

        #region ConfigureNamedHttpClient Tests

        [Test]
        public void ConfigureNamedHttpClient_BasicConfiguration_ConfiguresCreatedHttpClient()
        {
            // Test that the extension method can be called without throwing
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            var builder = services.AddHttpClient("client1");

            Assert.DoesNotThrow(() =>
            {
                builder.ConfigureNamedHttpClient(Consts.Name1, (sp, client) =>
                {
                    client.BaseAddress = new Uri("https://api1.example.com");
                    client.Timeout = TimeSpan.FromSeconds(30);
                });
            });

            // Also assert the configuration is actually applied to the HttpClient created via
            // the standard IHttpClientFactory for this client name, not just "didn't throw".
            using ServiceProvider provider = services.BuildServiceProvider();
            HttpClient client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("client1");

            Assert.That(client.BaseAddress, Is.EqualTo(new Uri("https://api1.example.com")));
            Assert.That(client.Timeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
        }

        [Test]
        public void ConfigureNamedHttpClient_MultipleClients_MethodCalled()
        {
            // Test that multiple configurations can be set
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            Assert.DoesNotThrow(() =>
            {
                services.AddHttpClient("client1")
                    .ConfigureNamedHttpClient(Consts.Name1, (sp, client) =>
                    {
                        client.BaseAddress = new Uri("https://api1.example.com");
                        client.Timeout = TimeSpan.FromSeconds(30);
                    });

                services.AddHttpClient("client2")
                    .ConfigureNamedHttpClient(Consts.Name2, (sp, client) =>
                    {
                        client.BaseAddress = new Uri("https://api2.example.com");
                        client.Timeout = TimeSpan.FromSeconds(60);
                    });
            });

            // Also assert each named client actually got its OWN configuration, not each
            // other's or a shared one.
            using ServiceProvider provider = services.BuildServiceProvider();
            IHttpClientFactory factory = provider.GetRequiredService<IHttpClientFactory>();
            HttpClient client1 = factory.CreateClient("client1");
            HttpClient client2 = factory.CreateClient("client2");

            Assert.That(client1.BaseAddress, Is.EqualTo(new Uri("https://api1.example.com")));
            Assert.That(client1.Timeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
            Assert.That(client2.BaseAddress, Is.EqualTo(new Uri("https://api2.example.com")));
            Assert.That(client2.Timeout, Is.EqualTo(TimeSpan.FromSeconds(60)));
        }

        [Test]
        public void ConfigureNamedHttpClient_NullBuilder_ThrowsArgumentNullException()
        {
            IHttpClientBuilder? builder = null;
            
            Assert.Throws<ArgumentNullException>(() =>
                builder!.ConfigureNamedHttpClient(Consts.Name1, (sp, client) => { }));
        }

        [Test]
        public void ConfigureNamedHttpClient_NullConfigureClient_ThrowsArgumentNullException()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            var builder = services.AddHttpClient("test");
            
            Assert.Throws<ArgumentNullException>(() =>
                builder.ConfigureNamedHttpClient(Consts.Name1, null!));
        }

        [Test]
        public void ConfigureNamedHttpClient_UsesIServiceProvider_MethodCalled()
        {
            // Test that IServiceProvider can be used in configuration
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddSingleton<TestService>(new TestService { Value = "TestValue" });
            var builder = services.AddHttpClient("testClient");
            
            Assert.DoesNotThrow(() =>
            {
                builder.ConfigureNamedHttpClient(Consts.Name1, (sp, client) =>
                {
                    var testService = sp.GetRequiredService<TestService>();
                    client.DefaultRequestHeaders.Add("X-Test-Header", testService.Value);
                });
            });
        }

        #endregion

        #region AddNamedHttpMessageHandler Tests

        [Test]
        public void AddNamedHttpMessageHandler_AddCustomHandler_HandlerIsInCreatedClientPipeline()
        {
            // Test that the handler configuration can be added without throwing
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedTransient<TestDelegatingHandler>(sp => new TestDelegatingHandler(), Consts.Name1);
            var builder = services.AddHttpClient("clientWithHandler");

            Assert.DoesNotThrow(() =>
            {
                builder.AddNamedHttpMessageHandler<TestDelegatingHandler>(Consts.Name1);
            });

            // Also assert the configured handler is actually part of the built handler pipeline
            // for this client, not just "didn't throw" while registering it.
            using ServiceProvider provider = services.BuildServiceProvider();
            IHttpMessageHandlerFactory handlerFactory = provider.GetRequiredService<IHttpMessageHandlerFactory>();
            HttpMessageHandler outerHandler = handlerFactory.CreateHandler("clientWithHandler");
            var chain = GetHandlerChain(outerHandler).ToList();

            Assert.That(chain.Any(h => h is TestDelegatingHandler), Is.True,
                "The configured handler must appear somewhere in the built handler pipeline.");
        }

        [Test]
        public void AddNamedHttpMessageHandler_NullBuilder_ThrowsArgumentNullException()
        {
            IHttpClientBuilder? builder = null;
            
            Assert.Throws<ArgumentNullException>(() =>
                builder!.AddNamedHttpMessageHandler<TestDelegatingHandler>(Consts.Name1));
        }

        [Test]
        public void AddNamedHttpMessageHandler_NamedHandler_MethodCalled()
        {
            // Test that named handlers can be configured
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedTransient<TestDelegatingHandler>(sp => new TestDelegatingHandler(), Consts.Name2);
            var builder = services.AddHttpClient("namedHandlerClient");
            
            Assert.DoesNotThrow(() =>
            {
                builder.AddNamedHttpMessageHandler<TestDelegatingHandler>(Consts.Name2);
            });
        }

        #endregion

        #region ConfigureNamedPrimaryHttpMessageHandler Tests

        [Test]
        public void ConfigureNamedPrimaryHttpMessageHandler_CustomHandler_HandlerIsPrimaryInCreatedClientPipeline()
        {
            // Test that the primary handler configuration can be added without throwing
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedTransient<TestHttpMessageHandler>(sp => new TestHttpMessageHandler(), Consts.Name1);
            var builder = services.AddHttpClient("clientWithPrimaryHandler");

            Assert.DoesNotThrow(() =>
            {
                builder.ConfigureNamedPrimaryHttpMessageHandler<TestHttpMessageHandler>(Consts.Name1);
            });

            // Also assert the configured handler actually became the PRIMARY handler for this
            // client's pipeline, not just "didn't throw" while registering it.
            using ServiceProvider provider = services.BuildServiceProvider();
            IHttpMessageHandlerFactory handlerFactory = provider.GetRequiredService<IHttpMessageHandlerFactory>();
            HttpMessageHandler outerHandler = handlerFactory.CreateHandler("clientWithPrimaryHandler");
            HttpMessageHandler innermost = GetHandlerChain(outerHandler).Last();

            Assert.That(innermost, Is.InstanceOf<TestHttpMessageHandler>(),
                "The configured handler must be the innermost (primary) handler of the pipeline.");
        }

        [Test]
        public void ConfigureNamedPrimaryHttpMessageHandler_NullBuilder_ThrowsArgumentNullException()
        {
            IHttpClientBuilder? builder = null;
            
            Assert.Throws<ArgumentNullException>(() =>
                builder!.ConfigureNamedPrimaryHttpMessageHandler<TestHttpMessageHandler>(Consts.Name1));
        }

        [Test]
        public void ConfigureNamedPrimaryHttpMessageHandler_GetHandlerByName_HandlerIsPrimaryInCreatedClientPipeline()
        {
            // Test that primary handlers can be configured by name
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedTransient<TestHttpMessageHandler>(sp => new TestHttpMessageHandler(), Consts.CustomName1);
            var builder = services.AddHttpClient("primaryHandlerClient");

            Assert.DoesNotThrow(() =>
            {
                builder.ConfigureNamedPrimaryHttpMessageHandler<TestHttpMessageHandler>(Consts.CustomName1);
            });

            using ServiceProvider provider = services.BuildServiceProvider();
            IHttpMessageHandlerFactory handlerFactory = provider.GetRequiredService<IHttpMessageHandlerFactory>();
            HttpMessageHandler outerHandler = handlerFactory.CreateHandler("primaryHandlerClient");
            HttpMessageHandler innermost = GetHandlerChain(outerHandler).Last();

            Assert.That(innermost, Is.InstanceOf<TestHttpMessageHandler>(),
                "The configured handler must be the innermost (primary) handler of the pipeline.");
        }

        #endregion

        #region AddNamedTypedClient Tests

        [Test]
        public void AddNamedTypedClient_WithImplementation_MethodCalled()
        {
            // Test that the typed client configuration can be added without throwing
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddNamedTransient<ITypedHttpClientFactory<TestTypedClient>>(sp => 
                new TestTypedClientFactory(), Consts.Name1);
            services.AddNamedSingleton<IHttpClientFactory>(sp => 
                sp.GetRequiredService<IHttpClientFactory>(), Consts.Name1);
            var builder = services.AddHttpClient("typedClient");
            
            Assert.DoesNotThrow(() =>
            {
                builder.AddNamedTypedClient<ITestTypedClient, TestTypedClient>(Consts.Name1);
            });
        }

        [Test]
        public void AddNamedTypedClient_WithFactory_MethodCalled()
        {
            // Test that the typed client with factory can be added without throwing
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            var builder = services.AddHttpClient("typedClientFactory");
            
            Assert.DoesNotThrow(() =>
            {
                builder.AddNamedTypedClient<ITestTypedClient>((httpClient, sp) =>
                {
                    return new TestTypedClient(httpClient);
                });
            });
        }

        [Test]
        public void AddNamedTypedClient_WithImplementation_NullBuilder_ThrowsArgumentNullException()
        {
            IHttpClientBuilder? builder = null;
            
            Assert.Throws<ArgumentNullException>(() =>
                builder!.AddNamedTypedClient<ITestTypedClient, TestTypedClient>(Consts.Name1));
        }

        [Test]
        public void AddNamedTypedClient_WithFactory_NullBuilder_ThrowsArgumentNullException()
        {
            IHttpClientBuilder? builder = null;
            
            Assert.Throws<ArgumentNullException>(() =>
                builder!.AddNamedTypedClient<ITestTypedClient>((client, sp) => new TestTypedClient(client)));
        }

        [Test]
        public void AddNamedTypedClient_WithFactory_NullFactory_ThrowsArgumentNullException()
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            var builder = services.AddHttpClient("test");
            
            Assert.Throws<ArgumentNullException>(() =>
                builder.AddNamedTypedClient<ITestTypedClient>(null!));
        }

        #endregion

        #region Test Helper Classes

        /// <summary>
        /// Walks the built <see cref="HttpMessageHandler"/> pipeline (as returned by
        /// <see cref="IHttpMessageHandlerFactory.CreateHandler"/>, which wraps everything in a
        /// lifetime-tracking outer handler and, by default, logging handlers) from outermost to
        /// innermost (the primary handler).
        /// </summary>
        internal static IEnumerable<HttpMessageHandler> GetHandlerChain(HttpMessageHandler handler)
        {
            var current = handler;
            while (current != null)
            {
                yield return current;
                current = (current as DelegatingHandler)?.InnerHandler;
            }
        }

        public class TestDelegatingHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Add custom logic here if needed for testing
                return base.SendAsync(request, cancellationToken);
            }
        }

        public class TestHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
        }

        public interface ITestTypedClient
        {
            HttpClient HttpClient { get; }
        }

        public class TestTypedClient : ITestTypedClient
        {
            public TestTypedClient(HttpClient httpClient)
            {
                HttpClient = httpClient;
            }

            public HttpClient HttpClient { get; }
        }

        public class TestTypedClientFactory : ITypedHttpClientFactory<TestTypedClient>
        {
            public TestTypedClient CreateClient(HttpClient httpClient)
            {
                return new TestTypedClient(httpClient);
            }
        }

        public class TestService
        {
            public string Value { get; set; } = string.Empty;
        }

        #endregion
    }
}
