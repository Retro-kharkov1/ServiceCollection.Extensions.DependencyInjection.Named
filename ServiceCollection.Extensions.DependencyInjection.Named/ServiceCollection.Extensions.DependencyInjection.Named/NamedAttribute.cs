using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCollection.Extensions.DependencyInjection.Named
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = true)]
    public class NamedAttribute: InjectAttribute
    {
        public string Name { get; internal set; }

        public NamedAttribute(string name) { 
            this.Name = name;
        }
    }
}
