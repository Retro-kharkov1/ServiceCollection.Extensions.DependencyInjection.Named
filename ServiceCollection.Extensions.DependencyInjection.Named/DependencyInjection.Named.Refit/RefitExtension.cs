using DependencyInjection.Named.Refit;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCollection.Extensions.DependencyInjection.Named.Refit
{
    public static class RefitExtension
    {

        /// <summary>
        /// Adds a Refit client to the DI container
        /// </summary>
        /// <typeparam name="T">Type of the Refit interface</typeparam>
        /// <param name="services">container</param>
        /// <param name="settings">Optional. Settings to configure the instance with</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddNamedRefitClient<T>(this IServiceCollection services, string name, RefitSettings settings = null) where T : class
            => AddNamedRefitClient<T>(services, name, _ => settings);

        /// <summary>
        /// Adds a Refit client to the DI container
        /// </summary>
        /// <typeparam name="T">Type of the Refit interface</typeparam>
        /// <param name="services">container</param>
        /// <param name="settingsAction">Optional. Action to configure refit settings.  This method is called once and only once, avoid using any scoped dependencies that maybe be disposed automatically.</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddNamedRefitClient<T>(this IServiceCollection services,
            string name,
            Func<IServiceProvider, RefitSettings> settingsAction = null) where T : class =>
            services
                .AddNamedSingleton(provider => new SettingsFor<T>(settingsAction?.Invoke(provider)), name)
                .AddNamedSingleton(provider => RequestBuilder.ForType<T>(provider.GetNamedService<SettingsFor<T>>(name).Settings), name)
                .AddNamedSingleton(provider => 
                    RestService.For<T>(provider.GetNamedService<HttpClient>(name), provider.GetNamedService<IRequestBuilder<T>>(name)), name)
                .AddHttpClient(name);
    }
}
