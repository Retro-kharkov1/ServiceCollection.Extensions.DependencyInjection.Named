using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceCollection.Extensions.DependencyInjection.Named.Refit;

namespace DependencyInjection.Named.Test.Refit
{
    public class AuthenticatedHttpClientHandlerTest
    {
        private const string OriginalScheme = "Bearer";
        private const string NewToken = "new-token-value";

        [Test]
        public void Constructor_NullGetToken_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AuthenticatedHttpClientHandler(null!, new RecordingHttpMessageHandler()));
        }

        [Test]
        public async Task SendAsync_RequestHasAuthorizationHeader_ReplacesTokenKeepingScheme()
        {
            var innerHandler = new RecordingHttpMessageHandler();
            var handler = new AuthenticatedHttpClientHandler(() => Task.FromResult(NewToken), innerHandler);
            using var invoker = new HttpMessageInvoker(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/ping");
            request.Headers.Authorization = new AuthenticationHeaderValue(OriginalScheme, "old-token");

            await invoker.SendAsync(request, CancellationToken.None);

            Assert.That(innerHandler.LastRequest, Is.Not.Null);
            Assert.That(innerHandler.LastRequest!.Headers.Authorization!.Scheme, Is.EqualTo(OriginalScheme));
            Assert.That(innerHandler.LastRequest!.Headers.Authorization!.Parameter, Is.EqualTo(NewToken));
        }

        [Test]
        public async Task SendAsync_RequestHasNoAuthorizationHeader_LeavesRequestUnchangedAndDoesNotCallGetToken()
        {
            var innerHandler = new RecordingHttpMessageHandler();
            var getTokenCalled = false;
            var handler = new AuthenticatedHttpClientHandler(() =>
            {
                getTokenCalled = true;
                return Task.FromResult(NewToken);
            }, innerHandler);
            using var invoker = new HttpMessageInvoker(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/ping");

            await invoker.SendAsync(request, CancellationToken.None);

            Assert.That(getTokenCalled, Is.False,
                "getToken must not be invoked when the outgoing request has no Authorization header.");
            Assert.That(innerHandler.LastRequest, Is.Not.Null);
            Assert.That(innerHandler.LastRequest!.Headers.Authorization, Is.Null);
        }

        private class RecordingHttpMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage? LastRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.LastRequest = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }
    }
}
