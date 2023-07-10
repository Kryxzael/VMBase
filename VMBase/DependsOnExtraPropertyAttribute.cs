using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMBase
{
    /// <summary>
    /// Makes this property depend on a property of an extra connection when in a VM.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public sealed class DependsOnExtraPropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the property this property depends on
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the connection key this property depends on
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Creates a new dependency
        /// </summary>
        /// <param name="propertyName">The name of the property this property depends on</param>
        public DependsOnExtraPropertyAttribute(string propertyName, string key)
        {
            PropertyName = propertyName;
            Key = key;
        }

    }
}
