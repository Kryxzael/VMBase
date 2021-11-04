using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using VMBase;

namespace VMBase
{
    /// <summary>
    /// Contains extension methods for IEnumerables relating to ViewModelBases
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Disposes all children of the provided property and registers the provided children as new children of the property on the provided view model
        /// </summary>
        /// <returns></returns>
        public static Child[] RegisterChildren<Child, Parent>(this IEnumerable<Child> children, ViewModelBase<Parent> parent, [CallerMemberName] string originatingProperty = "") where Child : IDisposable
        {
            return parent.RegisterChildren(children, originatingProperty);
        }
    }
}
