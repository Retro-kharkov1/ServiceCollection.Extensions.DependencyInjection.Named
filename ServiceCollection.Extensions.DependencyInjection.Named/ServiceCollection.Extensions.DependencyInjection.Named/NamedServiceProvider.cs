using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCollection.Extensions.DependencyInjection.Named
{
    public class NamedServiceProvider : IServiceProvider
    {
        //const string DefaultName = "AnyOtherServices";
        private readonly IServiceProvider services;
        private readonly Type[] injectAttributes;

        public NamedServiceProvider(IServiceProvider services, Type[] injectAttributes)
        {
            this.services = services;
            this.injectAttributes = injectAttributes;
        }

        public object GetService(Type serviceType)
        {
            var service = services.GetService(serviceType);

            InjectProperties(service);
            InjectNamed(service);
            return service;
        }

        private void InjectProperties(Object target)
        {
            var type = target.GetType();

            var candidateProperties = type.GetProperties(System.Reflection.BindingFlags.Public);

            var props = from p in candidateProperties
                        where injectAttributes.Any(a => p.GetCustomAttributes(a, true).Any())
                        select p;

            foreach (var prop in props)
            {
                prop.SetValue(target, services.GetService(prop.PropertyType));
            }
        }

        private void InjectNamed(Object target)
        { 
        
        }
    }
}
