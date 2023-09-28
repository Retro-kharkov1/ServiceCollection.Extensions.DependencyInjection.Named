using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCollection.Extensions.DependencyInjection.Named
{
    /// <summary>
    /// A factory for resolving named services.
    /// </summary>
    internal interface INamedServiceFactory
    {
        /// <summary>
        /// Gets the service type.
        /// </summary>
        Type ServiceType { get; }

        /// <summary>
        /// Resolves the service type matching the given name. 
        /// </summary>
        /// <param name="name">The name the service type is registered as.</param>
        /// <param name="provider">The service provider for retrieving service objects.</param>
        /// <returns>The <see cref="object"/></returns>
        object Resolve(string name, IServiceProvider provider);
    }
}
