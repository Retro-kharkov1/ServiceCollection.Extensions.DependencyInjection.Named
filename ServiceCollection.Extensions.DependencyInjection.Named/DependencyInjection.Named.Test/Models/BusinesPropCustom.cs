using DependencyInjection.Named.Test.Models;
using ServiceCollection.Extensions.DependencyInjection.Named;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analytics.Linq.Core.Test.Models
{
    public class BusinesPropCustom : IBusinesPropCustom
    {
        [Named(Consts.CustomName1)]
        public IRepository Repository { get; set; }         
        
        public string RepositoryName { get => this.Repository.Name; }
    }
}
