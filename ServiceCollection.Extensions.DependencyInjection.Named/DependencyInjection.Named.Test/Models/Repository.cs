using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analytics.Linq.Core.Test.Models
{
    public class Repository : IRepository
    {
        public Repository(string name) {
            this.Name = name;
        }
        public string Name { get; private set; }
    }
}
