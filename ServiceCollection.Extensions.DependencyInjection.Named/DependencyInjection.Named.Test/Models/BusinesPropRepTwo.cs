using ServiceCollection.Extensions.DependencyInjection.Named;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analytics.Linq.Core.Test.Models
{
    public class BusinesPropRepTwo : IBusinesPropTwo
    {
        [Named(Consts.Name2)]
        public IRepository Repository { get; set; }

        public string RepositoryName { get => this.Repository.Name; }
    }
}
