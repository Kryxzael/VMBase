using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using VMBase;

namespace VMBase
{
    /// <summary>
    /// Holds information about the attributes of members of VMBase types
    /// </summary>
    internal class TypeStore
    {
        /// <summary>
        /// Contains all stores known in the current environment
        /// </summary>
        private static readonly List<TypeStore> Stores = new List<TypeStore>();

        /// <summary>
        /// Gets the type of this type store
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the methods that will be executed when an item property changes on this type
        /// </summary>
        private MemberWithAttributes<HandleItemPropertyChangedAttribute>[] PropertyHandlers { get; }

        /// <summary>
        /// Gets the fields/properties that will be notified when an item property changes on this type
        /// </summary>
        private MemberWithAttributes<DependOnItemPropertyAttribute>[] PropertyDependencies { get; }

        /// <summary>
        /// Creates a new type store of the provided type
        /// </summary>
        /// <param name="type"></param>
        private TypeStore(Type type)
        {
            Type = type;

            PropertyHandlers = type.GetMethods()
                .Select(i => new MemberWithAttributes<HandleItemPropertyChangedAttribute>(i))
                .Where(i => i.Attributes.Any())
                .ToArray();

            PropertyDependencies = type.GetMembers()
                .Select(i => new MemberWithAttributes<DependOnItemPropertyAttribute>(i))
                .Where(i => i.Attributes.Any())
                .ToArray();
        }

        /// <summary>
        /// Runs a traversal on the provided view model, calling handlers and notifies properties.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vm"></param>
        internal void Notify<T>(ViewModelBase<T> vm, string property)
        {
            foreach (var i in PropertyHandlers)
            {
                if (i.Attributes.Any(i => i.PropertyName == property))
                    ((MethodInfo)i.Member).Invoke(vm, null);
            }

            foreach (var i in PropertyDependencies)
            {
                if (i.Attributes.Any(i => i.PropertyName == property))
                    vm.Notify(i.Member.Name);
            }
        }

        /// <summary>
        /// Gets the type store for the provided type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static TypeStore GetTypeStore(Type type)
        {
            TypeStore? output = Stores.SingleOrDefault(i => i.Type == type);

            if (output == null)
            {
                output = new TypeStore(type);
                Stores.Add(output);
            }

            return output;
        }


        /// <summary>
        /// Caches attribute info of members
        /// </summary>
        private struct MemberWithAttributes<A> where A : Attribute
        {
            /// <summary>
            /// Gets the member
            /// </summary>
            public MemberInfo Member { get; }

            /// <summary>
            /// Gets the attributes of the member
            /// </summary>
            public A[] Attributes { get; }

            /// <summary>
            /// Creates a new Member-With-Attribute definition and gets the relevant attributes for it
            /// </summary>
            /// <param name="member"></param>
            public MemberWithAttributes(MemberInfo member)
            {
                Member = member;
                Attributes = member.GetCustomAttributes<A>().ToArray();
            }
        }
    }
}
