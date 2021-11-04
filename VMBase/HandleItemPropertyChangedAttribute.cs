using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMBase
{
    /// <summary>
    /// Makes this function execute when the provided property of the item of a VM changes
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class HandleItemPropertyChangedAttribute : Attribute
    {
        /// <summary>
        /// Gets the property of the item this function will handle
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Creates a new handler for the provided property
        /// </summary>
        /// <param name="propertyName">The property of the item this function will handle</param>
        public HandleItemPropertyChangedAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

    }
}
