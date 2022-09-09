using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraLip.Common
{
    /// <summary>
    /// Standardization when a name is used
    /// </summary>
    public interface IName
    {
        /// <summary>
        /// The name of this instance.
        /// </summary>
        string Name { get; }
    }
}
