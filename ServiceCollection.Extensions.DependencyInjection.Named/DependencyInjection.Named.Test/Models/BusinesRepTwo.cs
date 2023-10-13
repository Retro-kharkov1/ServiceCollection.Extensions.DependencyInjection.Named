using ServiceCollection.Extensions.DependencyInjection.Named;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analytics.Linq.Core.Test.Models
{
    public class BusinesRepTwo : IBusinesTwo
    {
        public BusinesRepTwo([Named(Consts.Name2)]IRepository repository) {
            this.RepositoryName = repository.Name;
        }
        public string RepositoryName { get; private set; }
    }
}
