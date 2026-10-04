using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analytics.Linq.Core.Test.Models
{
    /// <summary>
    /// A single concrete implementation that gets registered under multiple different
    /// names (and/or multiple different <c>TService</c> types) in the cross-injection tests.
    /// Each instance carries a freshly-generated identity so tests can tell two resolved
    /// instances apart.
    /// </summary>
    public class RepositoryUnique : IRepository, IRepositoryAlt
    {
        public RepositoryUnique()
        {
            this.Name = Guid.NewGuid().ToString();
        }

        public string Name { get; private set; }
    }
}
