using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMBase
{
    /// <summary>
    /// Makes this property depend on a property of its item when in a VM.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public sealed class DependOnItemPropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the property this property depends on
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Creates a new dependency
        /// </summary>
        /// <param name="propertyName">The name of the property this property depends on</param>
        public DependOnItemPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

    }
}
