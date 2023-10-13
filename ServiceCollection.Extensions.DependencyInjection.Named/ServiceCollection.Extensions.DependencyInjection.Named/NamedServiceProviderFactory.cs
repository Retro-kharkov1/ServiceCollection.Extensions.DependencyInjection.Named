using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCollection.Extensions.DependencyInjection.Named
{
    public class NamedServiceProviderFactory : IServiceProviderFactory<NamedServiceProviderFactory.Builder>
    { 

        public class Builder
        {
            internal readonly IServiceCollection services;

            internal List<Type> attributeTypes = new List<Type>();

            public Builder(IServiceCollection services)
            {
                this.services = services;
            }

            public Builder AddInjectAttribute<A>()
                where A : Attribute
            {
                attributeTypes.Add(typeof(A));

                return this;
            }

            public IServiceProvider CreateServiceProvider()
                => new NamedServiceProvider(services.BuildServiceProvider(), attributeTypes.ToArray());
        }
         

        Builder IServiceProviderFactory<Builder>.CreateBuilder(IServiceCollection services)
        {
            return new Builder(services);
        }

        public IServiceProvider CreateServiceProvider(Builder containerBuilder)
        {
            return containerBuilder.CreateServiceProvider();
        }
    }
}
