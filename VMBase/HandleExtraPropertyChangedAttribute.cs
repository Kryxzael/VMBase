using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMBase
{
    /// <summary>
    /// Makes this function execute when the provided property of an extra connection of a VM changes
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class HandleExtraPropertyChangedAttribute : Attribute
    {
        /// <summary>
        /// Gets the property of the item this function will handle
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the connection key this property depends on
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Creates a new handler for the provided property
        /// </summary>
        /// <param name="propertyName">The property of the item this function will handle</param>
        public HandleExtraPropertyChangedAttribute(string propertyName, string key)
        {
            PropertyName = propertyName;
            Key = key;
        }

    }
}
