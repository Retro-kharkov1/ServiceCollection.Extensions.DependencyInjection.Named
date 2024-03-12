using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using ServiceCollection.Extensions.DependencyInjection.Named;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCollection.Extensions.DependencyInjection.Named
{
    public static class HttpClientBuilderNamedExtensions
    {
        public static IHttpClientBuilder ConfigureNamedHttpClient(this IHttpClientBuilder builder, string name,  Action<IServiceProvider, HttpClient> configureClient)
        {
            if (builder == null) 
                throw new ArgumentNullException(nameof(builder)); 

            if (configureClient == null) 
                throw new ArgumentNullException(nameof(configureClient)); 

            builder.Services.AddNamedTransient<IConfigureOptions<HttpClientFactoryOptions>>(services =>
            {
                return new ConfigureNamedOptions<HttpClientFactoryOptions>(builder.Name, (options) =>
                {
                    options.HttpClientActions.Add(client => configureClient(services, client));
                });
            }, name);

            return builder;
        }

        public static IHttpClientBuilder AddNamedHttpMessageHandler<THandler>(this IHttpClientBuilder builder, string name)
           where THandler : DelegatingHandler
        {
            if (builder == null) 
                throw new ArgumentNullException(nameof(builder)); 

            builder.Services.Configure<HttpClientFactoryOptions>(name, options =>
            {   
                options.HttpMessageHandlerBuilderActions.Add(b => b.AdditionalHandlers.Add(b.Services.GetNamedService<THandler>(name)));
            });

            return builder;
        }

        public static IHttpClientBuilder ConfigureNamedPrimaryHttpMessageHandler<THandler>(this IHttpClientBuilder builder, string name)
           where THandler : HttpMessageHandler
        {
            if (builder == null) 
                throw new ArgumentNullException(nameof(builder)); 

            builder.Services.Configure<HttpClientFactoryOptions>(builder.Name, options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(b => b.PrimaryHandler = b.Services.GetNamedService<THandler>(name));
            });

            return builder;
        }

        public static IHttpClientBuilder AddNamedTypedClient<TClient, TImplementation>(
           this IHttpClientBuilder builder, string name)
           where TClient : class
           where TImplementation : class, TClient
        {
            if (builder == null) 
                throw new ArgumentNullException(nameof(builder)); 

            return builder.AddNamedTypedClientCore<TClient, TImplementation>(name, validateSingleType: false);
        }
        internal static IHttpClientBuilder AddNamedTypedClientCore<TClient,  TImplementation>(
            this IHttpClientBuilder builder, string name,
            bool validateSingleType)
            where TClient : class
            where TImplementation : class, TClient
        { 
            builder.Services.AddNamedTransient(s => AddNamedTransientHelper<TClient, TImplementation>(name, s, builder), name);
            return builder;
        }
        
        private static TClient AddNamedTransientHelper<TClient, TImplementation>(string name, IServiceProvider s, IHttpClientBuilder builder)
            where TClient : class where TImplementation : class, TClient
        {
            IHttpClientFactory httpClientFactory = s.GetNamedService<IHttpClientFactory>(name);
            HttpClient httpClient = httpClientFactory.CreateClient(builder.Name);

            ITypedHttpClientFactory<TImplementation> typedClientFactory = s.GetNamedService<ITypedHttpClientFactory<TImplementation>>(name);
            return typedClientFactory.CreateClient(httpClient);
        }

        public static IHttpClientBuilder AddNamedTypedClient<TClient>(this IHttpClientBuilder builder, Func<HttpClient, IServiceProvider, TClient> factory)
         where TClient : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            } 

            builder.Services.AddNamedTransient<TClient>(s =>
            {
                IHttpClientFactory httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = httpClientFactory.CreateClient(builder.Name);

                return factory(httpClient, s);
            }, builder.Name);

            return builder;
        }
         

    }
}
